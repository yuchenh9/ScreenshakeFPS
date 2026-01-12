using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class aimRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    public Transform targetPosition; 
    public float recoilDuration = 0.05f; // 连发时建议缩短时间
    public float returnDuration = 0.1f;
    public AnimationCurve recoilCurve;
    public AnimationCurve returnCurve;
    
    private Vector3 originalLocalPos;
    private Coroutine recoilCoroutine;
    
    [Header("Firing Settings")]
    public float fireRate = 0.1f; 
    private float nextFireTime = 0f;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        ClickHandler();
    }

    void ClickHandler()
    {
        if(gameStat.Instance.isPaused) return;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    
    public void Fire()
    {
        // 关键点：停止旧的协程。这会连带停止该协程内正在执行的逻辑
        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
        }
        recoilCoroutine = StartCoroutine(FullRecoilSequence());
    }
    
    // 将两个阶段合并为一个协程，确保一次 Stop 就全部干净了
    IEnumerator FullRecoilSequence()
    {
        Vector3 currentPos = transform.localPosition;

        // 1. 向后退 (Recoil)
        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float t = recoilCurve.Evaluate(elapsed / recoilDuration);
            transform.localPosition = Vector3.Lerp(currentPos, targetPosition.localPosition, t);
            yield return null;
        }

        // 2. 回到初始位置 (Return)
        // 重新获取当前位置作为起点，防止位置突变
        Vector3 posAfterRecoil = transform.localPosition;
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = returnCurve.Evaluate(elapsed / returnDuration);
            transform.localPosition = Vector3.Lerp(posAfterRecoil, originalLocalPos, t);
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        recoilCoroutine = null; // 结束后清空引用
    }
}