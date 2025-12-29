using UnityEngine;

// 只有在编辑器模式下才引用这个命名空间
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabSpreader : MonoBehaviour
{
    public GameObject prefab;

    [Range(1, 20)] public int countX = 5;
    [Range(1, 20)] public int countY = 1;
    [Range(1, 20)] public int countZ = 5;

    public Vector3 spacing = new Vector3(2f, 2f, 2f);

#if UNITY_EDITOR
    // OnValidate 只在编辑器里修改 Inspector 参数时运行
    private void OnValidate()
    {
        // 延迟调用 Rebuild，避免在 OnValidate 中直接修改层级结构的警告
        EditorApplication.delayCall += Rebuild;
    }

    private void Rebuild()
    {
        EditorApplication.delayCall -= Rebuild;

        if (this == null || prefab == null) return;

        // 清理旧物体
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 生成格子
        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    Vector3 pos = new Vector3(x * spacing.x, y * spacing.y, z * spacing.z);
                    
                    // 使用 PrefabUtility 保留预制体关联
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    
                    instance.transform.SetParent(this.transform);
                    instance.transform.localPosition = pos;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.name = $"{prefab.name}_{x}_{y}_{z}";
                }
            }
        }
    }
#endif
}