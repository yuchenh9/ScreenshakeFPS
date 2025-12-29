using UnityEngine;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject redHeartPrefab;   
    public GameObject whiteHeartPrefab; 

    [Header("Manual Layout Settings")]
    public int maxHealth = 5;
    public Transform container;      // The hearts will be children of this
    public float spacing = 50f;      // Distance between hearts on X axis
    public Vector3 startOffset;      // Local starting position of the first heart

    private int lastKnownHP = -1;

    void Update()
    {
        // Check gameStat.Instance.hp
        if (gameStat.Instance.hp != lastKnownHP)
        {
            UpdateHearts(gameStat.Instance.hp);
            lastKnownHP = gameStat.Instance.hp;
        }
    }

    void UpdateHearts(int currentHP)
    {
        // 1. Clear existing hearts
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // 2. Draw hearts from left to right manually
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heartToSpawn = (i < currentHP) ? redHeartPrefab : whiteHeartPrefab;
            
            // Create the heart
            GameObject heart = Instantiate(heartToSpawn, container);

            // 3. Calculate position: Start + (Index * Spacing)
            // This moves the heart to the right along the X axis
            Vector3 localPos = startOffset + new Vector3(i * spacing, 0, 0);
            
            // Apply position to RectTransform (UI) or Transform (World/Sprite)
            if (heart.transform is RectTransform rect)
            {
                rect.anchoredPosition = localPos;
            }
            else
            {
                heart.transform.localPosition = localPos;
            }
        }
    }
}