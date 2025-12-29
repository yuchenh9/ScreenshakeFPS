using UnityEngine;
using UnityEngine.Pool;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lifetime = 0.8f;
    
    private TextMeshProUGUI _textMesh;
    private IObjectPool<GameObject> _pool;
    private float _currentTimer;
    private Color _originalColor;

    void Awake()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (_textMesh != null) _originalColor = _textMesh.color;
    }

    // 由 Manager 在 Create 时调用一次
    public void SetPool(IObjectPool<GameObject> pool)
    {
        _pool = pool;
    }

    // 每次从池子拿出时重置状态
    public void Initialize(float amount)
    {
        _currentTimer = lifetime;
        if (_textMesh != null)
        {
            _textMesh.SetText($"-{amount}");
            _textMesh.color = _originalColor; // 重置透明度
        }
    }

    void Update()
    {
        // 1. 向上漂浮
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        
        // 2. Billboard 效果：永远面向相机
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        // 3. 倒计时与淡出
        _currentTimer -= Time.deltaTime;
        
        if (_textMesh != null)
        {
            float alpha = Mathf.Clamp01(_currentTimer / lifetime);
            _textMesh.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, alpha);
        }

        // 4. 回收逻辑
        if (_currentTimer <= 0)
        {
            if (_pool != null) {
                _pool.Release(gameObject);
            } else {
                Destroy(gameObject); // 防御性销毁
            }
        }
    }
}