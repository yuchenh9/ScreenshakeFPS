using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class GunRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    // 现在只需设置 Z 轴的后坐力距离，例如 -0.1f
    public float recoilZOffset = -0.1f; 
    public float recoilDuration = 0.1f;
    public float returnDuration = 0.2f;
    public AnimationCurve recoilCurve;
    public AnimationCurve returnCurve;
    
    // originalPosition 也不再需要记录 Vector3，只需要记录初始的 Z
    private float originalZ; 
    private Coroutine recoilCoroutine;
    
    [Header("Audio Settings")]
    public AudioClip fireSound;
    private AudioSource audioSource;
    
    void Start()
    {
        // 记录枪支初始的本地 Z 轴坐标
        originalZ = transform.localPosition.z;
        audioSource = GetComponent<AudioSource>();
    }

    void Update(){
        ClickHandler();
    }

    void ClickHandler(){
        if(gameStat.Instance.isPaused) return;
        if(Mouse.current.leftButton.wasPressedThisFrame){
            Fire();
        }
    }
    
    public void Fire()
    {
        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
        }
        recoilCoroutine = StartCoroutine(RecoilAnimation());
        if(audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);
    }
    
    IEnumerator RecoilAnimation()
    {
        float targetZ = originalZ + recoilZOffset;
        
        // 向后退 (Recoil)
        yield return StartCoroutine(MoveZ(targetZ, recoilDuration, recoilCurve));
        
        // 回到初始 Z (Return)
        yield return StartCoroutine(MoveZ(originalZ, returnDuration, returnCurve));
    }
    
    // 关键修正：这个方法只修改 Z 轴
    IEnumerator MoveZ(float targetZ, float time, AnimationCurve curve)
    {
        float startZ = transform.localPosition.z;
        float elapsed = 0f;
        
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float curveValue = curve.Evaluate(t);
            
            // 获取当前的本地坐标
            Vector3 currentPos = transform.localPosition;
            // 只更新 Z 值，X 和 Y 保持不变（由 PlayerController 实时决定）
            float newZ = Mathf.Lerp(startZ, targetZ, curveValue);
            
            transform.localPosition = new Vector3(currentPos.x, currentPos.y, newZ);
            yield return null;
        }
        
        // 最后确保 Z 轴准确到达目标点，同时不改变 X 和 Y
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, targetZ);
    }
}