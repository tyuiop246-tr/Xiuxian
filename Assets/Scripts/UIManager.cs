using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private bool showMainMenu, showEncounter, showGameOver, showToast;
    private bool showCave, showQuests;
    private float toastTimer;

    private string realm = "炼气期";
    private int qi, qiNeed, distance, shield, stones, runStones;
    private string techs = "尚无功法";
    private string menuBest = "", overStats = "", toast = "";
    private int overDist, overScore, overBestDist;
    private string overRealm, overBestRealm;
    private TechniqueData[] choices;
    private System.Action<TechniqueData> onSelect;
    private bool canRevive;

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
        if (showCave) DrawCave(w, h);
        if (showQuests) DrawQuests(w, h);
        if (showToast) GUI.Label(new Rect(w * 0.2f, h * 0.07f, w * 0.6f, 50f), toast, toastStyle);
    }

    void DrawMainMenu(float w, float h)
    {
        GUI.DrawTexture(new Rect(0, 0, w, h), darkTex);
        GUI.Label(new Rect(w * 0.05f, h * 0.16f, w * 0.9f, h * 0.1f), "道途 · 无尽飞升", titleStyle);
        GUI.Label(new Rect(w * 0.05f, h * 0.27f, w * 0.9f, h * 0.05f), "修仙 · 跑酷 · Roguelike", subStyle);
        GUI.Label(new Rect(w * 0.05f, h * 0.33f, w * 0.9f, h * 0.05f), menuBest, hintStyle);
        GUI.Label(new Rect(w * 0.05f, h * 0.38f, w * 0.9f, h * 0.05f), "灵石：" + stones + "    复活符：" + MetaSystem.Revives, subStyle);

        if (GUI.Button(new Rect(w * 0.36f, h * 0.46f, w * 0.28f, h * 0.08f), "开始御剑  (空格)", buttonStyle))
            if (GameManager.instance != null) GameManager.instance.StartGame();
        if (GUI.Button(new Rect(w * 0.36f, h * 0.56f, w * 0.13f, h * 0.07f), "修仙洞府", buttonStyle)) showCave = true;
        if (GUI.Button(new Rect(w * 0.51f, h * 0.56f, w * 0.13f, h * 0.07f), "任务成就", buttonStyle)) showQuests = true;

        GUI.Label(new Rect(w * 0.05f, h * 0.72f, w * 0.9f, h * 0.12f),
            "A / D 或 ← → 横移     空格 跳跃     空格 开始     Shift 冲刺     C 切换男/女角色", hintStyle);
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
        GUI.Label(new Rect(w - 320, 52, 300, 30), "护体 " + shield + "   灵石 " + stones, right);
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
        var overTitle = Style(h * 0.07f, TextAnchor.MiddleCenter, new Color(1f, 0.4f, 0.4f));
        GUI.Label(new Rect(w * 0.05f, h * 0.12f, w * 0.9f, h * 0.1f), "道 心 破 碎", overTitle);
        GUI.Label(new Rect(w * 0.05f, h * 0.26f, w * 0.9f, h * 0.14f), overStats, hintStyle);

        // 广告复活 / 双倍结算（JD：广告变现）
        if (canRevive && GUI.Button(new Rect(w * 0.28f, h * 0.42f, w * 0.44f, h * 0.075f),
            "看视频原地复活 + 无敌3秒", buttonStyle))
        {
            if (GameManager.instance != null) GameManager.instance.WatchAdRevive();
        }
        if (runStones > 0 && GUI.Button(new Rect(w * 0.28f, h * 0.51f, w * 0.44f, h * 0.075f),
            "看视频灵石双倍结算（+" + runStones + "）", buttonStyle))
        {
            if (GameManager.instance != null) GameManager.instance.WatchAdDouble();
        }

        if (GUI.Button(new Rect(w * 0.33f, h * 0.64f, w * 0.34f, h * 0.09f), "再入仙道  (R)", buttonStyle))
            if (GameManager.instance != null) GameManager.instance.Restart();
    }

    // 修仙洞府（局外养成）
    void DrawCave(float w, float h)
    {
        GUI.DrawTexture(new Rect(0, 0, w, h), darkTex);
        GUI.Label(new Rect(w * 0.05f, h * 0.08f, w * 0.9f, h * 0.1f), "修 仙 洞 府", titleStyle);
        GUI.Label(new Rect(w * 0.05f, h * 0.18f, w * 0.9f, h * 0.06f), "灵石：" + MetaSystem.Stones, subStyle);

        string[] names = { "道心根基（初始护体）", "御剑术（初始移速）", "顿悟道痕（开局功法概率）" };
        int[] levels = { MetaSystem.HeartLv, MetaSystem.SpeedLv, MetaSystem.LuckLv };
        for (int i = 0; i < 3; i++)
        {
            float y = h * (0.3f + i * 0.16f);
            GUI.Label(new Rect(w * 0.08f, y, w * 0.5f, h * 0.05f), names[i] + "  Lv." + levels[i], hudStyle);
            int cost = MetaSystem.UpgradeCost(i);
            string label = cost < 0 ? "圆满" : "升级（" + cost + " 灵石）";
            if (cost >= 0 && GUI.Button(new Rect(w * 0.62f, y, w * 0.28f, h * 0.07f), label, buttonStyle))
            {
                if (MetaSystem.Upgrade(i)) ShowToast("升级成功！");
                else ShowToast("灵石不足");
            }
        }
        if (GUI.Button(new Rect(w * 0.36f, h * 0.85f, w * 0.28f, h * 0.07f), "返回", buttonStyle)) showCave = false;
    }

    // 任务与成就
    void DrawQuests(float w, float h)
    {
        GUI.DrawTexture(new Rect(0, 0, w, h), darkTex);
        GUI.Label(new Rect(w * 0.05f, h * 0.06f, w * 0.9f, h * 0.09f), "任 务 与 成 就", titleStyle);

        GUI.Label(new Rect(w * 0.06f, h * 0.16f, w * 0.88f, h * 0.05f), "— 每日任务 —", subStyle);
        string[] qNames = { "今日跑酷 2000 米", "吸收 50 个灵气", "完成 3 局" };
        for (int i = 0; i < 3; i++)
        {
            float y = h * (0.23f + i * 0.09f);
            bool claimed = MetaSystem.IsQuestClaimed(i);
            string s = qNames[i] + "  " + MetaSystem.QuestProgress(i) + "/" + MetaSystem.QuestGoal(i) + "  +" + MetaSystem.QuestReward(i) + "灵石";
            GUI.Label(new Rect(w * 0.08f, y, w * 0.6f, h * 0.05f), s, hudStyle);
            if (!claimed && GUI.Button(new Rect(w * 0.7f, y, w * 0.2f, h * 0.06f), "领取", buttonStyle))
            {
                if (MetaSystem.ClaimQuest(i)) ShowToast("任务奖励已领取！");
            }
            else if (claimed) GUI.Label(new Rect(w * 0.7f, y, w * 0.2f, h * 0.05f), "已领取", hintStyle);
        }

        GUI.Label(new Rect(w * 0.06f, h * 0.53f, w * 0.88f, h * 0.05f), "— 成就 —", subStyle);
        for (int i = 0; i < MetaSystem.AchievementNames.Length; i++)
        {
            float y = h * (0.6f + i * 0.06f);
            bool claimed = MetaSystem.IsAchievementClaimed(i);
            bool ok = MetaSystem.AchievementClaimable(i);
            string s = MetaSystem.AchievementNames[i] + "  +" + MetaSystem.AchievementRewards[i] + "灵石";
            GUI.Label(new Rect(w * 0.08f, y, w * 0.6f, h * 0.05f), s, hudStyle);
            if (!claimed && ok && GUI.Button(new Rect(w * 0.7f, y, w * 0.2f, h * 0.05f), "领取", buttonStyle))
            {
                if (MetaSystem.ClaimAchievement(i)) ShowToast("成就奖励已领取！");
            }
            else if (claimed) GUI.Label(new Rect(w * 0.7f, y, w * 0.2f, h * 0.045f), "已领取", hintStyle);
        }

        if (GUI.Button(new Rect(w * 0.36f, h * 0.93f, w * 0.28f, h * 0.06f), "返回", buttonStyle))
        {
            showQuests = false;
            stones = MetaSystem.Stones;
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
        stones = MetaSystem.Stones;
        menuBest = "最高记录：" + bestDistance + " 米 · " + bestRealm;
        if (MetaSystem.CanSignToday)
        {
            int reward = MetaSystem.SignToday();
            ShowToast(reward > 0 ? "签到成功！+" + reward + " 灵石" : "签到成功！获得复活符");
            stones = MetaSystem.Stones;
        }
    }

    public void HideMainMenu() { showMainMenu = false; }
    public void HideGameOver() { showGameOver = false; }

    public void UpdateHUD(string currentRealm, int currentQi, int needed, int dist, int currentShield, string techSummary, int currentStones, int currentRunStones)
    {
        realm = currentRealm;
        qi = currentQi;
        qiNeed = needed;
        distance = dist;
        shield = currentShield;
        techs = techSummary;
        stones = currentStones;
        runStones = currentRunStones;
    }

    public void ShowEncounter(TechniqueData[] data, System.Action<TechniqueData> callback)
    {
        choices = data;
        onSelect = callback;
        showEncounter = true;
    }
    public void HideEncounter() { showEncounter = false; }

    void RebuildOverStats(int earned, int totalStones)
    {
        overStats = "本局： " + overDist + " 米 · " + overRealm + " · 灵气 " + overScore +
            "\n本局灵石：+" + earned + "    当前灵石：" + totalStones +
            "\n历史最佳： " + overBestDist + " 米 · " + overBestRealm;
    }

    public void ShowGameOver(int dist, string currentRealm, int score, int bestDistance, string bestRealm, int earned, int totalStones, bool canAdRevive)
    {
        overDist = dist;
        overRealm = currentRealm;
        overScore = score;
        overBestDist = bestDistance;
        overBestRealm = bestRealm;
        RebuildOverStats(earned, totalStones);
        runStones = earned;
        stones = totalStones;
        canRevive = canAdRevive;
        showGameOver = true;
    }

    public void RefreshGameOverStones(int earned, int totalStones)
    {
        runStones = 0; // 双倍后隐藏按钮
        stones = totalStones;
        RebuildOverStats(earned, totalStones);
    }

    public void ShowToast(string message)
    {
        toast = message;
        showToast = true;
        toastTimer = 2f;
    }
}
