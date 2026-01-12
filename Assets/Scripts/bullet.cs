using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    private IObjectPool<GameObject> _pool;
    public float lifeTime = 5f;
    private float _timer;
    private Rigidbody _rb;

    [Header("Effects")]
    public ParticleSystem explosionPrefab;

    public void SetPool(IObjectPool<GameObject> pool) => _pool = pool;

    void Awake() => _rb = GetComponent<Rigidbody>();

    void OnEnable() => _timer = lifeTime;

    public void Launch(Vector3 direction, float speed) 
    {
        if (_rb == null) return;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.AddForce(direction * speed, ForceMode.Impulse);
    }

    void Update() 
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0) ReturnToPool();
    }
private void OnTriggerEnter(Collider other)
    {
        // 1. 播放爆炸特效
        PlayExplosion();

        // --- 核心修改：范围杀伤逻辑 ---
        float explosionRadius = 7f;  // 爆炸半径
        float explosionDamage = 10f; // 爆炸伤害

        // 获取爆炸中心点周围的所有碰撞体
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            enemyController enemy = hit.GetComponent<enemyController>();
            if (enemy != null)
            {
                // 计算爆炸对该敌人的推力方向：从爆炸中心指向敌人
                Vector3 explodeDirection = (hit.transform.position - transform.position).normalized;
                
                // 为了让喷溅效果更真实，可以混合一部分子弹原本的飞行惯性
                Vector3 bulletDir = _rb.linearVelocity.normalized;
                if (bulletDir == Vector3.zero) bulletDir = transform.forward;
                
                // 最终混合方向（70%爆炸推开 + 30%子弹惯性）
                Vector3 finalHitDir = Vector3.Lerp(explodeDirection, bulletDir, 0.3f);

                enemy.TakeDamage(explosionDamage, finalHitDir);
            }
        }

        // 3. 回收子弹
        ReturnToPool();
    }

    public void ExplodeAt(Vector3 position)
    {
        transform.position = position;
        if (explosionPrefab != null)
        {
            GameObject effect = Instantiate(explosionPrefab.gameObject, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        ReturnToPool();
    }

    private void PlayExplosion()
    {
        if (explosionPrefab != null)
        {
            GameObject effect = Instantiate(explosionPrefab.gameObject, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    private void ReturnToPool()
    {
        if (_pool != null) _pool.Release(gameObject);
        else Destroy(gameObject);
    }
}