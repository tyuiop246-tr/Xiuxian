using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 12;
    public int maxSize = 80;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (prefab != null) WarmUp(initialSize);
    }

    public void WarmUp(int count)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++) CreateOne();
    }

    private GameObject CreateOne()
    {
        GameObject go = Instantiate(prefab, transform);
        var po = go.GetComponent<PooledObject>();
        if (po == null) po = go.AddComponent<PooledObject>();
        po.owner = this;
        go.SetActive(false);
        pool.Enqueue(go);
        return go;
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject go = null;
        while (pool.Count > 0)
        {
            go = pool.Dequeue();
            if (go != null) break;
        }
        if (go == null) go = CreateOne();

        go.transform.position = position;
        go.transform.rotation = rotation;
        go.SetActive(true);

        var mover = go.GetComponent<MoveLeft>();
        if (mover != null) mover.ResetForPool();
        return go;
    }

    public void Release(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        if (pool.Count < maxSize) pool.Enqueue(go);
        else Destroy(go);
    }
}
