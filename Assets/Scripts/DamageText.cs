using UnityEngine;
using UnityEngine.Pool;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lifetime = 0.8f;
    
    // 关键修改：从 TextMeshProUGUI 改为 TextMeshPro
    private TextMeshPro _textMesh; 
    private IObjectPool<GameObject> _pool;
    private float _currentTimer;
    private Color _originalColor;

    void Awake()
    {
        // 同样修改这里的获取组件逻辑
        _textMesh = GetComponentInChildren<TextMeshPro>();
        
        if (_textMesh != null) 
        {
            _originalColor = _textMesh.color;
        }
        else 
        {
            Debug.LogError("在子物体中没找到 TextMeshPro (3D) 组件！请检查 Prefab。");
        }
    }

    public void SetPool(IObjectPool<GameObject> pool)
    {
        _pool = pool;
    }

    public void Initialize(float amount)
    {
        _currentTimer = lifetime;
        if (_textMesh != null)
        {
            // 取消注释以显示伤害数值
            _textMesh.SetText($"-{amount}");
            // 确保颜色 Alpha 回到 1
            Color c = _originalColor;
            c.a = 1f;
            _textMesh.color = c;
        }
    }
    void Update()
    {
        // 1. 向上漂浮
        transform.position += Vector3.up * (moveSpeed * Time.deltaTime);
        
        // 2. Billboard 效果 (让文字始终正对着摄像机)
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        // 3. 倒计时
        _currentTimer -= Time.deltaTime;

        // --- 注意这里：既然要完全不透明，不要在 Update 里反复 new Color ---
        // 已经在 Initialize 里设置过一次 a=1 了，这里其实不需要再写。
        // 如果你非要写，建议这样写以保证性能：
        // _textMesh.alpha = 1f; 

        // 4. 回收逻辑
        if (_currentTimer <= 0)
        {
            if (_pool != null) _pool.Release(gameObject);
            else Destroy(gameObject);
        }
    }
}