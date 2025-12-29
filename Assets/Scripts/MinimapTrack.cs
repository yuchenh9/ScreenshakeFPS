using UnityEngine;

public class MinimapTrack : MonoBehaviour
{
    private Transform player;
    private Transform enemy;

    [Header("设置")]
    public float mapRadius = 50f;      // 必须与小地图相机的 Orthographic Size 一致
    public float edgeBuffer = 0.8f;    // 0.9 表示在边缘内侧一点，防止图标一半被切掉
    public float iconHeight = 20f;     // 图标悬浮的高度（确保在相机视野内）

    void Start()
    {
        // 获取玩家（确保玩家有 "Player" 标签）
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // 获取敌人根物体
        enemy = transform.root; 
    }

    void LateUpdate()
    {
        if (player == null || enemy == null) return;

        // 1. 计算玩家到敌人的水平向量
        Vector3 rawOffset = enemy.position - player.position;
        rawOffset.y = 0; // 忽略高度差

        float distance = rawOffset.magnitude;

        // 2. 计算图标应该在的位置
        if (distance > mapRadius)
        {
            // 如果超出范围：将位置锁定在边缘
            // 方向 * 半径 * 缓冲系数
            Vector3 clampedOffset = rawOffset.normalized * mapRadius * edgeBuffer;
            
            // 将世界坐标设为：玩家位置 + 限制后的偏移
            transform.position = new Vector3(
                player.position.x + clampedOffset.x,
                iconHeight,
                player.position.z + clampedOffset.z
            );
        }
        else
        {
            // 如果在范围内：直接待在敌人头顶
            transform.position = new Vector3(
                enemy.position.x,
                iconHeight,
                enemy.position.z
            );
        }

        // 3. 锁定图标旋转（始终面向上方，且不随敌人旋转）
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}