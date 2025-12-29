using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class enemySpawn : MonoBehaviour
{
    public float spawnInterval=1f;
    public float nextSpawnTime=0f;
    public List<Transform> spawnPoints; // 在 Inspector 中拖入多个生成点
    public IObjectPool<GameObject> EnemyPool { get; private set; }
    public bool spawn=false;
    void OnValidate(){
        if(spawn){
            SpawnEnemy();
            spawn=false;
        }
    }
    void Awake()
    {
        EnemyPool = new ObjectPool<GameObject>(
            createFunc: CreateEnemy,
            actionOnGet: OnGetEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            defaultCapacity: 10,
            maxSize: 50
        );
    }
    void Start(){
        spawnPoints.Add(transform);
    }
    void Update(){
        if(!gameStat.Instance.finishedSpawning){
            if(Time.time>nextSpawnTime){
                nextSpawnTime=Time.time+spawnInterval;
                SpawnEnemy();
            }
        } 
    }

    private GameObject CreateEnemy()
    {
        // 假设你的敌人 Prefab 路径是 "Enemy"
        GameObject prefab = resourcesLoader.Instance.Load<GameObject>("Enemy");
        GameObject go = Instantiate(prefab);
        
        // 注入对象池引用，让敌人死后能把自己还给池子
        go.GetComponent<enemyController>().SetPool(EnemyPool);
        return go;
    }

    private void OnGetEnemy(GameObject obj)
    {
        // 随机选择一个生成点
        if (spawnPoints.Count > 0)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
            obj.transform.position = sp.position;
            obj.transform.rotation = sp.rotation;
        }

        
        obj.SetActive(true);
    }

    private void OnReleaseEnemy(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyEnemy(GameObject obj)
    {
        Destroy(obj);
    }

    // 外部调用：生成一个敌人
    public void SpawnEnemy()
    {
        EnemyPool.Get();
        
    }
}