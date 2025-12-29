using UnityEngine;
using UnityEngine.InputSystem; // Added for Mouse.current
using System.Collections;

public class aimRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    public Transform targetPosition; // The "Recoil Point" transform
    public float recoilDuration = 0.1f;
    public float returnDuration = 0.2f;
    public AnimationCurve recoilCurve;
    public AnimationCurve returnCurve;
    
    private Vector3 originalLocalPos;
    private Coroutine recoilCoroutine;
    
    void Start()
    {
        // Record the gun's starting local position
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        ClickHandler();
    }

    void ClickHandler()
    {
        // Ensure you are using the Input System Package
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
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
    }
    
    IEnumerator RecoilAnimation()
    {
        // Move towards the target transform's local position
        yield return StartCoroutine(MovePosition(targetPosition.localPosition, recoilDuration, recoilCurve));
        
        // Return to the original local position
        yield return StartCoroutine(MovePosition(originalLocalPos, returnDuration, returnCurve));
    }
    
    IEnumerator MovePosition(Vector3 targetPos, float time, AnimationCurve curve)
    {
        Vector3 startPos = transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float curveValue = curve.Evaluate(t);
            
            // Lerp the entire Vector3 (X, Y, and Z)
            transform.localPosition = Vector3.Lerp(startPos, targetPos, curveValue);
            yield return null;
        }
        
        transform.localPosition = targetPos;
    }
}