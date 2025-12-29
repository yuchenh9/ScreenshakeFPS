using UnityEngine;
using UnityEngine.Pool;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    private GameObject _damageTextPrefab;
    private IObjectPool<GameObject> _popupPool;

    // 属性：如果池子为空，尝试初始化它
    public IObjectPool<GameObject> PopupPool
    {
        get
        {
            if (_popupPool == null) InitPool();
            return _popupPool;
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 提前加载资源
        if (_damageTextPrefab == null)
        {
            _damageTextPrefab = resourcesLoader.Instance.Load<GameObject>("DamageText");
        }
    }

    private void InitPool()
    {
        // 再次检查资源，防止 Awake 还没走完就调用了
        if (_damageTextPrefab == null)
        {
            _damageTextPrefab = resourcesLoader.Instance.Load<GameObject>("DamageText");
        }

        _popupPool = new ObjectPool<GameObject>(
            createFunc: CreatePopupObject,
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    private GameObject CreatePopupObject()
    {
        GameObject go = Instantiate(_damageTextPrefab);
        DamageText script = go.GetComponent<DamageText>();
        if (script != null) 
        {
            // 这里传入 PopupPool 属性，确保触发初始化
            script.SetPool(PopupPool);
        }
        return go;
    }

    public void ShowDamage(Vector3 worldPosition, float amount)
    {
        // 使用属性 PopupPool 而不是字段 _popupPool
        if (PopupPool == null) 
        {
            Debug.LogError("无法初始化 DamagePopupPool！");
            return;
        }

        GameObject popupObj = PopupPool.Get();
        popupObj.transform.position = worldPosition + Vector3.up * 1.5f;

        DamageText script = popupObj.GetComponent<DamageText>();
        if (script != null) 
        {
            script.Initialize(amount);
        }
    }
}