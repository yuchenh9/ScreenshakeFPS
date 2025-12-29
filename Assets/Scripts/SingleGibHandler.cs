using UnityEngine;
using System.Collections;

public class SingleGibHandler : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private bool isSinking = false;

    public float groundY=0f;
    public float sinkDuration=30f;
    public float sinkDistance = 1.0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    public void ResetGib()
    {
        StopAllCoroutines();
        isSinking = false;
        
        transform.localPosition = initialLocalPos;
        transform.localRotation = initialLocalRot;
        
        rb.isKinematic = false;
        if (col != null) col.enabled = true;
        
        // 彻底清空残余速度
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        // 每个碎块独立判断自己是否触地
        if (!isSinking && transform.position.y <= groundY)
        {
            StartSink();
        }
    }

    private void StartSink()
    {
        isSinking = true;
        StartCoroutine(SinkRoutine());
    }

    IEnumerator SinkRoutine()
    {
        // 落地瞬间切断物理，防止其在斜坡滑动或滚动
        rb.isKinematic = true;
        if (col != null) col.enabled = false;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * sinkDistance;
        float elapsed = 0;

        // 执行你要求的极慢下沉（例如 20 秒）
        while (elapsed < sinkDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / sinkDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
}