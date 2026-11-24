using UnityEngine;

public class EffectsController : MonoBehaviour
{
    public static EffectsController Instance { get; private set; }

    private Camera cam;
    private Vector3 camBase;
    private bool shaking;
    private float shakeAmount;
    private float shakeTime;
    private Sprite particleSprite;

    public static void EnsureCreated()
    {
        if (Instance != null) return;
        var go = new GameObject("EffectsController");
        go.AddComponent<EffectsController>();
    }

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
        if (cam != null) camBase = cam.transform.position;
        try
        {
            var tex = Texture2D.whiteTexture;
            particleSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[道途] 粒子精灵创建失败，特效降级：" + e.Message);
        }
    }

    void Update()
    {
        if (!shaking || cam == null) return;
        shakeTime -= Time.unscaledDeltaTime;
        if (shakeTime <= 0f)
        {
            cam.transform.position = camBase;
            shaking = false;
            return;
        }
        Vector2 offset = Random.insideUnitCircle * shakeAmount;
        cam.transform.position = camBase + new Vector3(offset.x, offset.y, 0f);
    }

    public void Shake(float amount, float duration)
    {
        shakeAmount = Mathf.Max(shakeAmount, amount);
        shakeTime = Mathf.Max(shakeTime, duration);
        shaking = true;
    }

    public void Burst(Vector3 position, Color color, int count)
    {
        if (particleSprite == null) return;
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Particle");
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = particleSprite;
            sr.color = color;
            sr.sortingOrder = 20;
            go.transform.localScale = Vector3.one * Random.Range(0.06f, 0.16f);
            var bit = go.AddComponent<ParticleBit>();
            bit.velocity = Random.insideUnitCircle * Random.Range(2f, 6f);
            bit.life = Random.Range(0.35f, 0.75f);
        }
    }
}
