using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private bool showMainMenu;
    private bool showEncounter;
    private bool showGameOver;
    private bool showToast;
    private float toastTimer;

    private string realm = "炼气期";
    private int qi, qiNeed, distance, shield;
    private string techs = "尚无功法";
    private string menuBest = "";
    private string overStats = "";
    private string toast = "";
    private TechniqueData[] choices;
    private System.Action<TechniqueData> onSelect;

    private Texture2D darkTex, cardTex, barTex, qiTex;
    private Font chineseFont;
    private GUIStyle titleStyle, subStyle, hintStyle, hudStyle, buttonStyle, cardNameStyle, cardDescStyle, toastStyle, invisibleButton;

    public static void EnsureCreated()
    {
        if (Instance != null) return;
        new GameObject("UIManager").AddComponent<UIManager>();
    }

    void Awake()
    {
        Instance = this;
        chineseFont = Resources.Load<Font>("Fonts/DaoFont");
        if (chineseFont == null)
        {
            chineseFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 18);
            Debug.LogWarning("[道途] 未找到打包字体，回退系统字体（WebGL 下中文可能不显示）");
        }
        else
        {
            Debug.Log("[道途] 已加载打包中文字体");
        }
        darkTex = MakeTex(new Color(0.02f, 0.03f, 0.08f, 0.86f));
        cardTex = MakeTex(new Color(0.1f, 0.16f, 0.24f, 0.96f));
        barTex = MakeTex(new Color(0f, 0f, 0f, 0.5f));
        qiTex = MakeTex(new Color(0.3f, 0.95f, 0.5f, 1f));
    }

    static Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    void InitStyles(float h)
    {
        titleStyle = Style(h * 0.065f, TextAnchor.MiddleCenter, new Color(0.8f, 0.95f, 1f));
        subStyle = Style(h * 0.032f, TextAnchor.MiddleCenter, new Color(0.6f, 0.8f, 0.9f));
        hintStyle = Style(h * 0.026f, TextAnchor.MiddleCenter, new Color(0.75f, 0.8f, 0.85f));
        hudStyle = Style(h * 0.03f, TextAnchor.UpperLeft, Color.white);
        buttonStyle = Style(h * 0.034f, TextAnchor.MiddleCenter, Color.white);
        cardNameStyle = Style(h * 0.038f, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.5f));
        cardDescStyle = Style(h * 0.026f, TextAnchor.UpperCenter, Color.white);
        toastStyle = Style(h * 0.035f, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.4f));

        invisibleButton = new GUIStyle(GUI.skin.button);
        invisibleButton.normal.background = null;
        invisibleButton.hover.background = null;
        invisibleButton.active.background = null;
        invisibleButton.border = new RectOffset(0, 0, 0, 0);
    }

    GUIStyle Style(float size, TextAnchor anchor, Color color)
    {
        var s = new GUIStyle(GUI.skin.label);
        s.font = chineseFont;
        s.fontSize = Mathf.Max(12, Mathf.RoundToInt(size));
        s.alignment = anchor;
        s.normal.textColor = color;
        s.wordWrap = true;
        return s;
    }

    void OnGUI()
    {
        float w = Screen.width;
        float h = Screen.height;
        InitStyles(h);

        if (showMainMenu) DrawMainMenu(w, h);
        else DrawHUD(w, h);

        if (showEncounter) DrawEncounter(w, h);
        if (showGameOver) DrawGameOver(w, h);
        if (showToast) GUI.Label(new Rect(w * 0.2f, h * 0.12f, w * 0.6f, 50f), toast, toastStyle);
    }

    void DrawMainMenu(float w, float h)
    {
        GUI.DrawTexture(new Rect(0, 0, w, h), darkTex);
        GUI.Label(new Rect(w * 0.05f, h * 0.2f, w * 0.9f, h * 0.12f), "道途 · 无尽飞升", titleStyle);
        GUI.Label(new Rect(w * 0.05f, h * 0.33f, w * 0.9f, h * 0.06f), "修仙 · 跑酷 · Roguelike", subStyle);
        GUI.Label(new Rect(w * 0.05f, h * 0.42f, w * 0.9f, h * 0.06f), menuBest, hintStyle);

        if (GUI.Button(new Rect(w * 0.33f, h * 0.54f, w * 0.34f, h * 0.09f), "开始御剑  (空格)", buttonStyle))
        {
            if (GameManager.instance != null) GameManager.instance.StartGame();
        }
        GUI.Label(new Rect(w * 0.05f, h * 0.68f, w * 0.9f, h * 0.1f),
            "A / D 或 ← → 横移     空格 腾空     Shift 冲刺     C 切换男/女角色", hintStyle);
    }

    void DrawHUD(float w, float h)
    {
        GUI.Label(new Rect(20, 16, w * 0.45f, 40), realm, hudStyle);
        GUI.Label(new Rect(20, 52, w * 0.45f, 30), "灵气 " + qi + "/" + qiNeed, hudStyle);
        GUI.DrawTexture(new Rect(22, 86, 200, 12), barTex);
        float ratio = qiNeed > 0 ? Mathf.Clamp01(qi / (float)qiNeed) : 0f;
        GUI.DrawTexture(new Rect(22, 86, 200 * ratio, 12), qiTex);

        var right = Style(h * 0.03f, TextAnchor.UpperRight, Color.white);
        GUI.Label(new Rect(w - 320, 16, 300, 40), "距离 " + distance + " 米", right);
        GUI.Label(new Rect(w - 320, 52, 300, 30), "护体 " + shield, right);
        GUI.Label(new Rect(20, h - 45, w * 0.8f, 30), "功法：" + techs, hintStyle);
    }

    void DrawEncounter(float w, float h)
    {
        GUI.DrawTexture(new Rect(0, 0, w, h), darkTex);
        GUI.Label(new Rect(w * 0.05f, h * 0.1f, w * 0.9f, h * 0.1f), "奇 遇 · 三 选 一", titleStyle);
        GUI.Label(new Rect(w * 0.05f, h * 0.19f, w * 0.9f, h * 0.05f), "点击卡片或按 1 / 2 / 3 选择功法", hintStyle);

        for (int i = 0; i < 3; i++)
        {
            TechniqueData tech = choices != null && i < choices.Length ? choices[i] : null;
            if (tech == null) continue;
            Rect card = new Rect(w * (0.035f + i * 0.323f), h * 0.28f, w * 0.30f, h * 0.52f);
            GUI.DrawTexture(card, cardTex);
            int idx = i;
            if (GUI.Button(card, GUIContent.none, invisibleButton)) Select(idx);

            GUI.Label(new Rect(card.x, card.y + h * 0.03f, card.width, h * 0.05f), "[ " + (i + 1) + " ]", hintStyle);
            GUI.Label(new Rect(card.x, card.y + h * 0.1f, card.width, h * 0.08f), tech.techniqueName, cardNameStyle);
            GUI.Label(new Rect(card.x, card.y + h * 0.2f, card.width, h * 0.05f), tech.category.ToString(), hintStyle);
            GUI.Label(new Rect(card.x + 12, card.y + h * 0.3f, card.width - 24, h * 0.17f), tech.description, cardDescStyle);
        }
    }

    void DrawGameOver(float w, float h)
    {
        GUI.DrawTexture(new Rect(0, 0, w, h), darkTex);
        var overTitle = Style(h * 0.075f, TextAnchor.MiddleCenter, new Color(1f, 0.4f, 0.4f));
        GUI.Label(new Rect(w * 0.05f, h * 0.2f, w * 0.9f, h * 0.12f), "道 心 破 碎", overTitle);
        GUI.Label(new Rect(w * 0.05f, h * 0.39f, w * 0.9f, h * 0.16f), overStats, hintStyle);
        if (GUI.Button(new Rect(w * 0.33f, h * 0.62f, w * 0.34f, h * 0.09f), "再入仙道  (R)", buttonStyle))
        {
            if (GameManager.instance != null) GameManager.instance.Restart();
        }
    }

    void Update()
    {
        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            if (toastTimer <= 0f) showToast = false;
        }
        if (showEncounter)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Select(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) Select(2);
        }
    }

    void Select(int index)
    {
        if (choices == null || index < 0 || index >= choices.Length || choices[index] == null) return;
        var tech = choices[index];
        var callback = onSelect;
        showEncounter = false;
        if (callback != null) callback(tech);
    }

    public void ShowMainMenu(int bestDistance, string bestRealm)
    {
        showMainMenu = true;
        menuBest = "最高记录：" + bestDistance + " 米 · " + bestRealm;
    }

    public void HideMainMenu() { showMainMenu = false; }

    public void UpdateHUD(string currentRealm, int currentQi, int needed, int dist, int currentShield, string techSummary)
    {
        realm = currentRealm;
        qi = currentQi;
        qiNeed = needed;
        distance = dist;
        shield = currentShield;
        techs = techSummary;
    }

    public void ShowEncounter(TechniqueData[] data, System.Action<TechniqueData> callback)
    {
        choices = data;
        onSelect = callback;
        showEncounter = true;
    }

    public void HideEncounter() { showEncounter = false; }

    public void ShowGameOver(int dist, string currentRealm, int score, int bestDistance, string bestRealm)
    {
        overStats = "本局： " + dist + " 米 · " + currentRealm + " · 灵气 " + score +
            "\n历史最佳： " + bestDistance + " 米 · " + bestRealm;
        showGameOver = true;
    }

    public void ShowToast(string message)
    {
        toast = message;
        showToast = true;
        toastTimer = 2f;
    }
}
