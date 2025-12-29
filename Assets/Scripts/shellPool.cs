using UnityEngine;
using System.Collections.Generic;

public class shellPool : MonoBehaviour
{
    public static shellPool Instance;
    
    void Awake()
    {
        if(Instance == null){
            Instance = this;
        }else{
            Destroy(this.gameObject);
        }
    }

    public GameObject shellPrefab;
    public Transform startPosition;
    public Transform shootingDirection;
    
    [Header("Settings")]
    public int maxShells = 10; // Lowered for easier testing
    public float shootForce = 10f;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    public void ShootShell()
    {
        GameObject shell;
        
        // DEBUG: Track the count
        Debug.Log($"[Pool] Current Count: {poolQueue.Count} / Max: {maxShells}");

        if (poolQueue.Count < maxShells)
        {
            shell = CreateNewShell();
            Debug.Log("[Pool] Created NEW shell object.");
        }
        else
        {
            shell = poolQueue.Dequeue();
            Debug.Log("[Pool] RECYCLING oldest shell from Queue.");
        }

        PrepareShell(shell);
        poolQueue.Enqueue(shell);

        if (shell.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // FIX: Use .forward (direction) instead of .position (a point in space)
            Vector3 forceDir = shootingDirection.forward; 
            rb.AddForce(forceDir * shootForce, ForceMode.Impulse);
            
            Debug.Log($"[Pool] Applied force in direction: {forceDir}");
        }
        else
        {
            Debug.LogError("[Pool] Shell prefab is missing a Rigidbody!");
        }
    }

    private GameObject CreateNewShell()
    {
        return Instantiate(shellPrefab);
    }

    private void PrepareShell(GameObject shell)
    {
        shell.SetActive(false); 
        shell.transform.position = startPosition.position;
        shell.transform.rotation = startPosition.rotation;
        shell.SetActive(true);
    }
}