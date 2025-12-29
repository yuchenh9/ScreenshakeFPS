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
        PlayExplosion();

        enemyController enemy = other.GetComponent<enemyController>();
        if (enemy != null)
        {
            // --- 关键修改：获取子弹飞行方向并传入 ---
            Vector3 hitDirection = _rb.linearVelocity.normalized;
            // 如果是射线检测先一步杀死了敌人，这里可能已经取不到速度，可以改用 transform.forward
            if (hitDirection == Vector3.zero) hitDirection = transform.forward;
            
            enemy.TakeDamage(25f, hitDirection);
        }

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