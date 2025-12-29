using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance{get;private set;}
    void Awake(){
        if(Instance==null){
            Instance=this;
        }else{
            Destroy(gameObject);
        }
    }
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Generate a random offset
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; // Wait for next frame
        }

        transform.localPosition = originalPos;
    }
}