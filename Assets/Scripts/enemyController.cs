using UnityEngine;
using UnityEngine.Pool;

public class enemyController : MonoBehaviour
{
    public Transform player;
   private IObjectPool<GameObject> _pool;
    public float maxHealth = 100f;
    private float currentHealth;
    public float speed = 10f;
    public float attackWaitTime = 5f;
    private float attackTimer = 0f;
    public void SetPool(IObjectPool<GameObject> pool) => _pool = pool;
    [Header("烂肉和血液贴花")]
    public GameObject gibsPrefab; 
    //public GameObject bloodSplatPrefab; 

    void OnEnable()
    {
        // 每次从池子取出时，重置血量
        currentHealth = maxHealth;

        gameStat.Instance.enemyCount++;
        //Debug.Log("[Enemy] 重新刷出，血量已满");
    }
    void OnDisable()
    {
        gameStat.Instance.enemyCount--;
        gameStat.Instance.score++;
    }
    void Start(){
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    // 被子弹打中时调用
    void Update()
    {
        transform.LookAt(player);
        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
    }
private Vector3 lastHitDirection; // 记录最后一次被击中的方向

public void TakeDamage(float damage, Vector3 hitDirection) // 增加方向参数
{
    currentHealth -= damage;
    lastHitDirection = hitDirection; // 记录方向

    if (DamagePopupManager.Instance != null)
    {
        DamagePopupManager.Instance.ShowDamage(transform.position, damage);
    }

    if (currentHealth <= 0) Die();
}private void Die()
{
    // 将最后一次击中的方向传给 Splash
    Splash(lastHitDirection); 
    
    if (_pool != null) _pool.Release(this.gameObject);
    else Destroy(gameObject);
}

// 修改 Splash 接收方向
public void Splash(Vector3 direction)
{
    // 1. 生成碎肉
    if (GibsPool.Instance != null)
    {
        GibsPool.Instance.SpawnGibs(transform.position, direction);
    }

    // 2. 射线检测：顺着子弹方向稍微向下偏一点
    RaycastHit hit;
    // 计算喷溅向量：子弹方向 + 一定的重力下坠方向
    Vector3 splashDir = (direction.normalized + Vector3.down * 0.5f).normalized;

    if (Physics.Raycast(transform.position, splashDir, out hit, 10f))
    {
        if (BloodDecalPool.Instance != null)
        {
            // 将子弹方向传入，用于旋转和拉长贴花
            BloodDecalPool.Instance.SpawnSlantedDecal(hit.point, hit.normal, direction);
        }
    }
}

    // 如果敌人碰到玩家也可以销毁（示例）
    private void OnCollisionEnter(Collision collision)
    {
        if(Time.time-attackTimer>attackWaitTime){
            if (collision.gameObject.CompareTag("Player"))
            {
                gameStat.Instance.hp--;
                attackTimer = Time.time;
            }
        }
    }
}