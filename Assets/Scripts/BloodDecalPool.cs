using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class BloodDecalPool : MonoBehaviour
{
    public static BloodDecalPool Instance;

    [Header("Settings")]
    public GameObject bloodDecalPrefab;
    public int maxDecals = 30; // 场景中允许存在的最大贴花数
    public float decalSize = 2f;

    // 使用 Queue 替代 Unity 默认的 ObjectPool 以实现强制循环
    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 通用的准备方法：重置位置、旋转并激活
    private void PrepareDecal(GameObject decal, Vector3 position, Quaternion rotation)
    {
        decal.SetActive(false); // 先关闭，防止位置移动时的视觉闪烁
        decal.transform.position = position;
        decal.transform.rotation = rotation;
        decal.SetActive(true);
    }

    private GameObject GetOrCreateDecal()
    {
        GameObject decal;
        if (poolQueue.Count < maxDecals)
        {
            // 池子还没满，创建新的
            decal = Instantiate(bloodDecalPrefab);
        }
        else
        {
            // 池子满了，取出最旧的一个（队列头部）
            decal = poolQueue.Dequeue();
        }
        // 注意：这里不直接 Enqueue，在外面设置好后再 Enqueue 入队尾
        return decal;
    }

    // 普通喷溅
    public void SpawnDecal(Vector3 position, Vector3 normal)
    {
        GameObject decal = GetOrCreateDecal();
        
        Vector3 spawnPos = position + normal * 0.01f;
        Quaternion spawnRot = Quaternion.LookRotation(-normal);
        
        PrepareDecal(decal, spawnPos, spawnRot);
        decal.transform.Rotate(Vector3.forward, Random.Range(0, 360));

        // 重新设置回默认大小（防止被 SlantedDecal 的大小影响）
        if (decal.TryGetComponent(out DecalProjector projector))
        {
            projector.size = new Vector3(decalSize, decalSize, 1.0f);
        }

        poolQueue.Enqueue(decal);
    }

    // 斜向拉长喷溅
    public void SpawnSlantedDecal(Vector3 position, Vector3 normal, Vector3 hitDirection)
    {
        GameObject decal = GetOrCreateDecal();
        
        Vector3 spawnPos = position + normal * 0.05f;
        Quaternion spawnRot = Quaternion.LookRotation(-normal, hitDirection);

        PrepareDecal(decal, spawnPos, spawnRot);

        if (decal.TryGetComponent(out DecalProjector projector))
        {
            float stretchFactor = Random.Range(3f, 6f);
            projector.size = new Vector3(decalSize, decalSize * stretchFactor, 1.0f);
        }

        poolQueue.Enqueue(decal);
    }
}