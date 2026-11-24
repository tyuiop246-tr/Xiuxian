using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    private Rigidbody2D rb;
    private int jumpCount = 0;
    private int maxJumps = 1;

    private float dashTimer = 0f;
    private float dashCooldown = 0f;
    private float dashDir = 1f;
    private float invincibleTimer = 0f;
    private float thornsCooldown = 0f;
    private float orbitTimer = 0f;
    private float thunderTimer = 0f;

    private bool hasDash, hasThorns, hasOrbit, hasThunder, hasMagnet, invincibleOnBreak;
    private float magnetRange = 0f;
    private float orbitRadius = 2f;
    private float orbitInterval = 0.6f;
    private float thunderInterval = 12f;
    private int qiGain = 1;

    private int shields = 0;
    public int Shields => shields;

    private readonly Dictionary<TechniqueEffect, int> stacks = new Dictionary<TechniqueEffect, int>();
    private readonly List<string> techNames = new List<string>();

    private SpriteRenderer spriteRenderer;
    private Sprite maleSprite;
    private Sprite femaleSprite;
    private bool isFemale = false;
    public bool IsFemale => isFemale;

    void Awake()
    {
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
        maleSprite = PixelArtLoader.Load("Art/Hero_Male", 192f);
        femaleSprite = PixelArtLoader.Load("Art/Hero_Female", 192f);
        Debug.Log("[道途] 角色贴图加载：" + (maleSprite != null ? "成功" : "失败"));
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.freezeRotation = true;
        }
        orbitTimer = orbitInterval;
        thunderTimer = thunderInterval;
        ApplyCharacterSprite();
    }

    void ApplyCharacterSprite()
    {
        if (spriteRenderer == null) return;
        Sprite target = isFemale ? femaleSprite : maleSprite;
        if (target != null)
        {
            spriteRenderer.sprite = target;
            spriteRenderer.drawMode = SpriteDrawMode.Simple;
        }
    }

    void ToggleCharacter()
    {
        isFemale = !isFemale;
        ApplyCharacterSprite();
        if (UIManager.Instance != null)
            UIManager.Instance.ShowToast(isFemale ? "切换角色：白衣仙子" : "切换角色：白衣剑修");
    }

    // 应用局外洞府养成加成
    public void ApplyMetaBonuses()
    {
        moveSpeed = 8f + MetaSystem.StartSpeedBonus;
        shields = MetaSystem.StartShields;
        Debug.Log("[道途] 洞府加成：移速+" + MetaSystem.StartSpeedBonus + " 护体+" + shields);
    }

    // 广告复活：原地复活 + 短暂无敌 + 清除周围障碍
    public void ReviveWithInvincibility(float seconds)
    {
        invincibleTimer = seconds;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            transform.position = new Vector3(-5f, 0f, 0f);
        }
        foreach (var c in Physics2D.OverlapCircleAll(transform.position, 3f))
        {
            if (c.CompareTag("Obstacle")) ReleaseOrDestroy(c.gameObject);
        }
        EffectsController.Instance.Burst(transform.position, new Color(1f, 0.9f, 0.3f), 20);
    }

    void Update()
    {
        if (GameManager.instance == null) return;

        if (Input.GetKeyDown(KeyCode.C)) ToggleCharacter();

        if (GameManager.instance.state == GameState.Ready)
        {
            float readyInput = Input.GetAxis("Horizontal");
            if (Mathf.Abs(readyInput) > 0.1f || Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Return))
            {
                GameManager.instance.StartGame();
            }
        }

        if (GameManager.instance.state != GameState.Playing) return;

        if (transform.position.y < -6f)
        {
            transform.position = new Vector3(transform.position.x, 0f, 0f);
            if (rb != null) rb.velocity = Vector2.zero;
        }

        invincibleTimer -= Time.deltaTime;
        thornsCooldown -= Time.deltaTime;
        dashCooldown -= Time.deltaTime;

        float moveInput = Input.GetAxis("Horizontal");

        if (spriteRenderer != null)
        {
            if (moveInput < -0.1f) spriteRenderer.flipX = true;
            else if (moveInput > 0.1f) spriteRenderer.flipX = false;
        }

        if (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            rb.velocity = new Vector2(dashDir * (moveSpeed + 14f), rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
            if (hasDash && Input.GetKeyDown(KeyCode.LeftShift) && dashCooldown <= 0f)
            {
                dashDir = Mathf.Abs(moveInput) > 0.1f ? Mathf.Sign(moveInput) : 1f;
                dashTimer = 0.15f;
                dashCooldown = 1.2f;
                EffectsController.Instance.Shake(0.05f, 0.1f);
            }
        }

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && jumpCount < maxJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
        }

        if (hasMagnet) ApplyMagnet();
        if (hasOrbit)
        {
            orbitTimer -= Time.deltaTime;
            if (orbitTimer <= 0f)
            {
                orbitTimer = orbitInterval;
                SweepOrbit();
            }
        }
        if (hasThunder)
        {
            thunderTimer -= Time.deltaTime;
            if (thunderTimer <= 0f)
            {
                thunderTimer = thunderInterval;
                CallThunder();
            }
        }
    }

    void ApplyMagnet()
    {
        foreach (var c in Physics2D.OverlapCircleAll(transform.position, magnetRange))
        {
            if (c.CompareTag("Qi"))
                c.transform.position = Vector3.MoveTowards(c.transform.position, transform.position, 9f * Time.deltaTime);
        }
    }

    void SweepOrbit()
    {
        foreach (var c in Physics2D.OverlapCircleAll(transform.position, orbitRadius))
        {
            if (c.CompareTag("Obstacle"))
            {
                EffectsController.Instance.Burst(c.transform.position, new Color(0.4f, 0.9f, 1f), 6);
                ReleaseOrDestroy(c.gameObject);
            }
        }
    }

    void CallThunder()
    {
        foreach (var go in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            EffectsController.Instance.Burst(go.transform.position, new Color(1f, 0.9f, 0.3f), 8);
            ReleaseOrDestroy(go);
        }
        EffectsController.Instance.Shake(0.2f, 0.25f);
        UIManager.Instance.ShowToast("天雷引！");
    }

    static void ReleaseOrDestroy(GameObject go)
    {
        var pooled = go.GetComponent<PooledObject>();
        if (pooled != null) pooled.Release();
        else Destroy(go);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Qi") && GameManager.instance != null)
        {
            GameManager.instance.AddQi(qiGain);
            EffectsController.Instance.Shake(0.04f, 0.1f);
            EffectsController.Instance.Burst(other.transform.position, new Color(0.3f, 1f, 0.4f), 7);
            ReleaseOrDestroy(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            HandleObstacleHit(collision.gameObject);
        }
    }

    void HandleObstacleHit(GameObject obstacle)
    {
        if (invincibleTimer > 0f) return;

        if (hasThorns && thornsCooldown <= 0f)
        {
            thornsCooldown = 1f;
            EffectsController.Instance.Burst(obstacle.transform.position, new Color(1f, 0.5f, 0.1f), 12);
            ReleaseOrDestroy(obstacle);
            UIManager.Instance.ShowToast("反伤罡气！");
            return;
        }

        if (shields > 0)
        {
            shields--;
            EffectsController.Instance.Shake(0.2f, 0.3f);
            EffectsController.Instance.Burst(transform.position, new Color(0.3f, 0.7f, 1f), 14);
            ReleaseOrDestroy(obstacle);
            UIManager.Instance.ShowToast("护体灵气碎裂！");
            return;
        }

        if (GameManager.instance != null) GameManager.instance.GameOver();
    }

    public void OnRealmBreakthrough()
    {
        transform.localScale *= 1.03f;
        moveSpeed += 0.25f;
        if (invincibleOnBreak) invincibleTimer = 1.5f;
    }

    public void ApplyTechnique(TechniqueData tech)
    {
        int stack = 0;
        stacks.TryGetValue(tech.effect, out stack);
        stack++;
        stacks[tech.effect] = stack;

        switch (tech.effect)
        {
            case TechniqueEffect.MoveSpeed: moveSpeed += tech.value; break;
            case TechniqueEffect.JumpForce: jumpForce += tech.value; break;
            case TechniqueEffect.DoubleJump: maxJumps = 2; break;
            case TechniqueEffect.Dash: hasDash = true; break;
            case TechniqueEffect.Shield: shields += Mathf.RoundToInt(tech.value); break;
            case TechniqueEffect.Thorns: hasThorns = true; break;
            case TechniqueEffect.OrbitSword:
                hasOrbit = true;
                orbitRadius = Mathf.Max(orbitRadius, tech.value);
                break;
            case TechniqueEffect.Thunder:
                hasThunder = true;
                thunderInterval = Mathf.Max(4f, tech.value);
                break;
            case TechniqueEffect.Magnet:
                hasMagnet = true;
                magnetRange = Mathf.Max(magnetRange, tech.value);
                break;
            case TechniqueEffect.InvincibleOnBreak: invincibleOnBreak = true; break;
            case TechniqueEffect.MaxLives: shields += 1; break;
            case TechniqueEffect.QiGain: qiGain += Mathf.RoundToInt(tech.value); break;
        }

        string label = tech.techniqueName + (stack > 1 ? " x" + stack : "");
        techNames.Add(label);
        Debug.Log("[道途] 习得功法：" + label);
    }

    public string GetTechniqueSummary()
    {
        if (techNames.Count == 0) return "尚无功法";
        var sb = new StringBuilder();
        for (int i = 0; i < techNames.Count && i < 5; i++)
        {
            if (i > 0) sb.Append("  ");
            sb.Append(techNames[i]);
        }
        if (techNames.Count > 5) sb.Append(" ...");
        return sb.ToString();
    }
}
