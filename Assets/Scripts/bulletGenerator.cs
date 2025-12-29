using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.InputSystem;
using System.Collections;

public class BulletGenerator : MonoBehaviour
{
    public Transform startPosition;
    public Transform shootingDirection;
    public IObjectPool<GameObject> BulletPool { get; private set; }
    public float maxShootDistance = 50f;

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
    }

    private GameObject CreateBullet()
    {
        GameObject prefab = resourcesLoader.Instance.Load<GameObject>("Bullet");
        GameObject go = Instantiate(prefab);
        go.GetComponent<Bullet>().SetPool(BulletPool);
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
                    StartCoroutine(MyCoroutine());
                }
            }
        }
        else
        {
            if (bulletScript != null)
            {
                bulletScript.Launch(direction, 100f);
            }
        }
    }

    IEnumerator MyCoroutine()
    {
        yield return new WaitForSeconds(2f); 
    }

    void Update() => ClickHandler();

    void ClickHandler()
    {
        if(gameStat.Instance.isPaused) return;
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
            if(PlayerController.Instance != null)
                StartCoroutine(PlayerController.Instance.Shake(0.1f, 0.2f));
        }
    }
}