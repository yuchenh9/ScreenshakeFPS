using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class GunRecoil : MonoBehaviour
{
    public static GunRecoil Instance;

    [Header("Recoil Settings")]
    public float recoilZOffset = -0.1f; 
    public float recoilDuration = 0.05f; // 连发时建议缩短，增加打击感
    public float returnDuration = 0.1f;
    public AnimationCurve recoilCurve;
    public AnimationCurve returnCurve;
    
    [Header("Firing Settings")]
    public float fireRate = 0.1f; // 连发速度
    private float nextFireTime = 0f;

    private float originalZ; 
    private Coroutine recoilCoroutine;
    
    [Header("Audio Settings")]
    public AudioClip fireSound;
    private AudioSource audioSource;

    [Header("VFX Settings")]
    public ParticleSystem muzzleFlash; 

    void Awake(){
        Instance = this;
    }

    void Start()
    {
        originalZ = transform.localPosition.z;
        audioSource = GetComponent<AudioSource>();
    }

    void Update(){
        ClickHandler();
    }

    void ClickHandler(){
        if(gameStat.Instance.isPaused) return;

        // 修改点：使用 .isPressed 实现连发
        if(Mouse.current.leftButton.isPressed )
        {
            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
        } 
        else if(Mouse.current.rightButton.wasPressedThisFrame)
        {
            Fire();
        }
    }
    
    public void Fire()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(); 
            muzzleFlash.Play();
        }

        // 停止之前的协程，确保后坐力立即重置
        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
        }
        recoilCoroutine = StartCoroutine(RecoilSequence());

        if(audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound,PlayerController.Instance.volumn);
    }
    
    // 整合为一个协程，解决 StopCoroutine 停止不彻底的问题
    IEnumerator RecoilSequence()
    {
        float startZ = transform.localPosition.z;
        float targetZ = originalZ + recoilZOffset;

        // 1. 后退阶段
        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float t = recoilCurve.Evaluate(elapsed / recoilDuration);
            float newZ = Mathf.Lerp(startZ, targetZ, t);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, newZ);
            yield return null;
        }

        // 2. 复位阶段
        float currentZ = transform.localPosition.z;
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = returnCurve.Evaluate(elapsed / returnDuration);
            float newZ = Mathf.Lerp(currentZ, originalZ, t);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, newZ);
            yield return null;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, originalZ);
        recoilCoroutine = null;
    }
}