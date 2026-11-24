using UnityEngine;

public class ParticleBit : MonoBehaviour
{
    public Vector3 velocity;
    public float life = 0.6f;
    private float maxLife;
    private SpriteRenderer sr;

    void Awake()
    {
        maxLife = life;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        transform.position += velocity * dt;
        velocity *= 0.93f;
        life -= dt;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Clamp01(life / maxLife);
            sr.color = c;
        }
        if (life <= 0f) Destroy(gameObject);
    }
}
