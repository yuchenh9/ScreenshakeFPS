using UnityEngine;

public class GibsGroupController : MonoBehaviour
{
    private SingleGibHandler[] gibHandlers;

    [Header("下沉设置")]
    public float sinkDuration = 20f;  
    public float sinkDistance = 1.0f; 

    [Header("力的大小")]
    public float explodeForce = 15f; // 使用 Impulse 模式建议力度设大一点

    void Awake()
    {
        gibHandlers = GetComponentsInChildren<SingleGibHandler>();
    }

    public void Explode(Vector3 originPos, Vector3 hitDirection, float floorY)
    {
        transform.position = originPos;
        transform.rotation = Quaternion.identity;

        foreach (var gib in gibHandlers)
        {
            gib.groundY = floorY;
            gib.sinkDuration = sinkDuration;
            gib.sinkDistance = sinkDistance;
            gib.ResetGib();

            Rigidbody rb = gib.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 增加随机偏移量，使飞散更自然
                Vector3 scatter = Random.insideUnitSphere * 0.5f;
                Vector3 force = (hitDirection.normalized + scatter) * explodeForce + Vector3.up * 5f;
                
                // 改回使用 Impulse 模式，受质量（Mass）影响
                rb.AddForce(force, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            }
        }
    }
}