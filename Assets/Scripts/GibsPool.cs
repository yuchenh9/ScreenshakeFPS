using UnityEngine;
using System.Collections.Generic;

public class GibsPool : MonoBehaviour
{
    public static GibsPool Instance;
    public GameObject gibsGroupPrefab; // 挂有 GibsGroupController 的预制体
    public int poolSize = 10;          // 注意：如果下沉需要20秒，池子建议设大点（如30）

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    void Awake() => Instance = this;

    public void SpawnGibs(Vector3 pos, Vector3 direction)
    {
        GameObject group;
        if (poolQueue.Count < poolSize)
        {
            group = Instantiate(gibsGroupPrefab);
        }
        else
        {
            group = poolQueue.Dequeue();
        }

        // 假设地面在敌人坐标下方一点点，或者根据具体场景调整
        float floorY = pos.y - 0.5f; 

        if (group.TryGetComponent(out GibsGroupController controller))
        {
            controller.Explode(pos, direction, floorY);
        }

        poolQueue.Enqueue(group);
    }
}