using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.InputSystem;
using System.Collections;

public class BulletGenerator : MonoBehaviour
{
    public Transform startPosition;
    public Transform shootingDirection;
    public IObjectPool<GameObject> BulletPool { get; private set; }
    public IObjectPool<GameObject> BulletPoolRight { get; private set; }
    public float maxShootDistance = 50f;
    public GameObject bulletRightPrefab;
    public GameObject bulletLeftPrefab;
    [Header("连发设置")]
    public float fireRate = 0.1f; // 两次射击之间的时间间隔（秒）
    private float nextFireTime = 0f; // 下一次可以射击的时间

    void Awake()
    {
        BulletPool = new ObjectPool<GameObject>(
            createFunc: CreateBullet,
            actionOnGet: OnGetBullet,
            actionOnRelease: OnReleaseBullet,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: true, 
            defaultCapacity: 20, 
            maxSize: 100
        );
        BulletPoolRight = new ObjectPool<GameObject>(
            createFunc: CreateBulletRight,
            actionOnGet: OnGetBullet,
            actionOnRelease: OnReleaseBullet,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: true, 
            defaultCapacity: 20, 
            maxSize: 100
        );
    }

    private GameObject CreateBullet()
    {
        GameObject go = Instantiate(bulletLeftPrefab);
        go.GetComponent<Bullet>().SetPool(BulletPool);
        return go;
    }
    private GameObject CreateBulletRight()
    {
        GameObject go = Instantiate(bulletRightPrefab);
        go.GetComponent<Bullet>().SetPool(BulletPoolRight);
        return go;
    }
    private void OnGetBullet(GameObject obj)
    {
        obj.transform.position = startPosition.position;
        obj.transform.rotation = startPosition.rotation;
        obj.SetActive(true);
    }

    private void OnReleaseBullet(GameObject obj) => obj.SetActive(false);
    private void OnDestroyBullet(GameObject obj) => Destroy(obj);
    

    public void Shoot()
    {
        GunRecoil.Instance.Fire();
        if (shellPool.Instance != null) shellPool.Instance.ShootShell();
        if (shootingDirection == null) return;

        Vector3 origin = startPosition.position;
        Vector3 direction = shootingDirection.forward; // 这是子弹飞行的方向

        GameObject bulletObj = BulletPool.Get();
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, maxShootDistance))
        {
            if (bulletScript != null)
            {
                bulletScript.ExplodeAt(hit.point); 
            }

            if (hit.collider.CompareTag("Enemy"))
            {
                enemyController enemy = hit.collider.GetComponent<enemyController>();
                if (enemy != null)
                {   
                    // --- 关键修改：在这里传入 direction ---
                    enemy.TakeDamage(25f, direction); 
                }
            }
        }
        
    }
    public void ShootRight()
    {
        if (shellPool.Instance != null) shellPool.Instance.ShootShell();
        if (shootingDirection == null) return;

        Vector3 origin = startPosition.position;
        Vector3 direction = shootingDirection.forward; // 这是子弹飞行的方向
        GameObject bulletObj = BulletPoolRight.Get();
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Launch(direction, 1f);
        }
    }

    void Update() => ClickHandler();

    void ClickHandler()
    {
        if(gameStat.Instance.isPaused) return;

        // --- 左键逻辑：自动连发 (isPressed) ---
        if(Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;

                if(PlayerController.Instance != null)
                    StartCoroutine(PlayerController.Instance.Shake(0.05f, 0.1f));
            }
        } 
        // --- 右键逻辑：点射 (wasPressedThisFrame) ---
        // 注意：这里使用 else if 保证了互斥性（左键按住时右键无效）
        else if(Mouse.current.rightButton.wasPressedThisFrame)
        {
            // 如果你希望点射不受连发 fireRate 的限制，直接调用即可
            ShootRight();
            
            // 如果你希望点射后也有一点点强制间隔，可以取消下面这行的注释
            // nextFireTime = Time.time + fireRate; 

            if(PlayerController.Instance != null)
                StartCoroutine(PlayerController.Instance.Shake(0.1f, 0.2f));
        }
    }
}