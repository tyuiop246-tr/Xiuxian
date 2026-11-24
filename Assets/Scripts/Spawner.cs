using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject qiPrefab;
    public GameObject obstaclePrefab;

    public float spawnRate = 1.5f;
    public float spawnX = 15f;
    public float minY = -1.5f;
    public float maxY = 1.5f;
    public float obstacleChance = 0.3f;

    private float nextSpawnTime = 0f;
    private ObjectPool qiPool;
    private ObjectPool obstaclePool;

    void Start()
    {
        qiPool = CreatePool("QiPool", qiPrefab);
        obstaclePool = CreatePool("ObstaclePool", obstaclePrefab);
    }

    ObjectPool CreatePool(string name, GameObject prefab)
    {
        if (prefab == null) return null;
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var pool = go.AddComponent<ObjectPool>();
        pool.prefab = prefab;
        pool.initialSize = 10;
        pool.maxSize = 60;
        pool.WarmUp(pool.initialSize);
        return pool;
    }

    void Update()
    {
        if (GameManager.instance == null || GameManager.instance.state != GameState.Playing) return;
        if (Time.time < nextSpawnTime) return;

        float speedUp = 1f - Mathf.Clamp01(GameManager.instance.distance / 6000f) * 0.4f;
        nextSpawnTime = Time.time + Mathf.Max(0.55f, spawnRate * speedUp);
        SpawnObject();
    }

    void SpawnObject()
    {
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(spawnX, randomY, 0f);
        float chance = obstacleChance + Mathf.Clamp01(GameManager.instance.distance / 5000f) * 0.2f;

        if (Random.value < chance)
        {
            if (obstaclePool != null) obstaclePool.Get(spawnPos, Quaternion.identity);
            else if (obstaclePrefab != null) Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            if (qiPool != null) qiPool.Get(spawnPos, Quaternion.identity);
            else if (qiPrefab != null) Instantiate(qiPrefab, spawnPos, Quaternion.identity);
        }
    }
}
