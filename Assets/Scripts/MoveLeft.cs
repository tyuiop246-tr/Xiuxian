using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    public float speed = 10f;

    [Header("地面循环用")]
    public bool loop = false;
    public float loopLeftX = -50f;
    public float loopRightX = 50f;

    [Header("生成物用")]
    public float destroyX = -12f;
    public bool spin = false;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.state != GameState.Playing) return;

        transform.Translate(Vector2.left * speed * Time.deltaTime);
        if (spin) transform.Rotate(0f, 0f, 90f * Time.deltaTime);

        if (loop)
        {
            if (transform.position.x < loopLeftX)
                transform.position = new Vector3(loopRightX, transform.position.y, 0f);
        }
        else if (transform.position.x < destroyX)
        {
            var pooled = GetComponent<PooledObject>();
            if (pooled != null) pooled.Release();
            else Destroy(gameObject);
        }
    }

    public void ResetForPool()
    {
        transform.rotation = Quaternion.identity;
        transform.localScale = originalScale;
    }
}
