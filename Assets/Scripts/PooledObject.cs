using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public ObjectPool owner;

    public void Release()
    {
        if (owner != null) owner.Release(gameObject);
        else Destroy(gameObject);
    }
}
