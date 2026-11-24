using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Ready, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isGameOver = false;
    public int score = 0;

    public GameState state = GameState.Ready;
    public float scrollSpeed = 10f;
    public float encounterInterval = 300f;

    public static readonly string[] RealmNames =
    {
        "炼气期", "筑基期", "金丹期", "元婴期", "化神期",
        "炼虚期", "合体期", "大乘期", "渡劫期", "仙人"
    };

    public int realmIndex = 0;
    public int qi = 0;
    public float distance = 0f;
    public int techniquesGained = 0;
    public int reviveCount = 0;

    public int bestDistance = 0;
    public int bestRealm = 0;
    public int lastRunStones = 0;

    private float nextEncounterDistance;
    private TechniqueLibrary library;
    private bool runCommitted = false;

    const string KeyDistance = "Dao_BestDistance";
    const string KeyRealm = "Dao_BestRealm";

    public int QiNeeded => 10 + realmIndex * 8;
    public string CurrentRealm => RealmNames[Mathf.Min(realmIndex, RealmNames.Length - 1)];
    public int StonesEarned => Mathf.FloorToInt(distance / 100f) + qi / 10;

    void Awake()
    {
        instance = this;
        UIManager.EnsureCreated();
        EffectsController.EnsureCreated();
        if (FindObjectOfType<PixelBackground>() == null)
            new GameObject("PixelBackground").AddComponent<PixelBackground>();
        LoadSave();
        library = Resources.Load<TechniqueLibrary>("TechniqueLibrary");
    }

    void Start()
    {
        state = GameState.Ready;
        nextEncounterDistance = encounterInterval;
        UIManager.Instance.ShowMainMenu(bestDistance, RealmNames[Mathf.Min(bestRealm, RealmNames.Length - 1)]);
        RefreshHUD();
    }

    void Update()
    {
        if (state == GameState.Playing)
        {
            distance += scrollSpeed * Time.deltaTime;
            if (distance >= nextEncounterDistance)
            {
                nextEncounterDistance += encounterInterval;
                TriggerEncounter();
            }
            RefreshHUD();
        }

        if (state == GameState.Ready && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            StartGame();
        if (state == GameState.GameOver && Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    void RefreshHUD()
    {
        if (UIManager.Instance == null) return;
        int shield = PlayerController.Instance != null ? PlayerController.Instance.Shields : 0;
        UIManager.Instance.UpdateHUD(CurrentRealm, qi, QiNeeded, Mathf.FloorToInt(distance), shield,
            PlayerController.Instance != null ? PlayerController.Instance.GetTechniqueSummary() : "",
            MetaSystem.Stones, StonesEarned);
    }

    public void StartGame()
    {
        if (state != GameState.Ready) return;
        state = GameState.Playing;
        isGameOver = false;
        techniquesGained = 0;
        reviveCount = 0;
        runCommitted = false;
        Time.timeScale = 1f;
        UIManager.Instance.HideMainMenu();
        UIManager.Instance.HideGameOver();
        if (PlayerController.Instance != null) PlayerController.Instance.ApplyMetaBonuses();
        if (MetaSystem.TryRollStartTechnique())
        {
            var t = PickThree()[0];
            if (t != null && PlayerController.Instance != null)
            {
                PlayerController.Instance.ApplyTechnique(t);
                techniquesGained++;
                UIManager.Instance.ShowToast("顿悟！开局功法：" + t.techniqueName);
            }
        }
        EffectsController.Instance.Shake(0.06f, 0.15f);
    }

    public void Restart()
    {
        if (state == GameState.GameOver) CommitRun(1);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddScore() { AddQi(1); }

    public void AddQi(int amount)
    {
        if (state != GameState.Playing) return;
        score += amount;
        qi += amount;
        while (qi >= QiNeeded)
        {
            qi -= QiNeeded;
            realmIndex++;
            EffectsController.Instance.Shake(0.12f, 0.3f);
            EffectsController.Instance.Burst(PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero,
                new Color(0.4f, 1f, 0.7f), 18);
            if (PlayerController.Instance != null) PlayerController.Instance.OnRealmBreakthrough();
            UIManager.Instance.ShowToast("突破！" + CurrentRealm);
        }
        RefreshHUD();
    }

    void TriggerEncounter()
    {
        state = GameState.Paused;
        Time.timeScale = 0f;
        UIManager.Instance.ShowEncounter(PickThree(), OnTechniqueChosen);
    }

    TechniqueData[] PickThree()
    {
        var result = new TechniqueData[3];
        if (library == null || library.techniques == null || library.techniques.Length == 0)
        {
            for (int i = 0; i < 3; i++) result[i] = TechniqueFallback.Create(i);
            return result;
        }
        var used = new System.Collections.Generic.HashSet<int>();
        for (int i = 0; i < 3 && i < library.techniques.Length; i++)
        {
            int idx, guard = 0;
            do { idx = Random.Range(0, library.techniques.Length); guard++; }
            while (used.Contains(idx) && guard < 50);
            used.Add(idx);
            result[i] = library.techniques[idx];
        }
        return result;
    }

    void OnTechniqueChosen(TechniqueData tech)
    {
        if (tech == null) return;
        if (PlayerController.Instance != null) PlayerController.Instance.ApplyTechnique(tech);
        techniquesGained++;
        Time.timeScale = 1f;
        state = GameState.Playing;
        UIManager.Instance.HideEncounter();
        UIManager.Instance.ShowToast("获得功法：" + tech.techniqueName);
        RefreshHUD();
    }

    public int ReviveStoneCost => 10;

    // 复活条件：境界仍在炼气期以上，可倒退一级换复活
    public bool CanReviveByRealm => state == GameState.GameOver && !runCommitted && reviveCount < 3 && realmIndex > 0;
    // 复活条件：持有至少 10 灵石
    public bool CanReviveByStone => state == GameState.GameOver && !runCommitted && reviveCount < 3 && MetaSystem.Stones >= ReviveStoneCost;

    public string PrevRealmName => RealmNames[Mathf.Max(0, realmIndex - 1)];

    // 代价一：境界倒退一级，灵气压到当前上限以下避免立刻反弹突破
    public void ReviveByRealm()
    {
        if (!CanReviveByRealm) return;
        realmIndex--;
        qi = Mathf.Min(qi, QiNeeded - 1);
        DoRevive("境界倒退至" + CurrentRealm + "！无敌 3 秒");
    }

    // 代价二：扣除 10 灵石
    public void ReviveByStone()
    {
        if (!CanReviveByStone) return;
        MetaSystem.AddStones(-ReviveStoneCost);
        DoRevive("消耗 " + ReviveStoneCost + " 灵石复活！无敌 3 秒");
    }

    void DoRevive(string message)
    {
        reviveCount++;
        state = GameState.Playing;
        isGameOver = false;
        Time.timeScale = 1f;
        UIManager.Instance.HideGameOver();
        if (PlayerController.Instance != null) PlayerController.Instance.ReviveWithInvincibility(3f);
        EffectsController.Instance.Shake(0.2f, 0.4f);
        UIManager.Instance.ShowToast(message);
        RefreshHUD();
    }

    // 广告双倍结算
    public void WatchAdDouble()
    {
        if (state != GameState.GameOver || runCommitted || lastRunStones <= 0) return;
        CommitRun(2);
        UIManager.Instance.ShowToast("广告双倍！本局共 +" + lastRunStones + " 灵石");
        UIManager.Instance.RefreshGameOverStones(lastRunStones, MetaSystem.Stones);
    }

    // 全局唯一的本局结算出口：广告复活不结算，避免重复发奖 / 重复记任务
    public void CommitRun(int multiplier)
    {
        if (runCommitted || state == GameState.Ready) return;
        runCommitted = true;
        int earned = StonesEarned * Mathf.Max(1, multiplier);
        lastRunStones = earned;
        MetaSystem.AddStones(earned);
        MetaSystem.RecordRun(Mathf.FloorToInt(distance), score, realmIndex, techniquesGained);
    }

    public void GameOver()
    {
        if (state == GameState.GameOver) return;
        state = GameState.GameOver;
        isGameOver = true;
        EffectsController.Instance.Shake(0.4f, 0.6f);
        if (PlayerController.Instance != null)
            EffectsController.Instance.Burst(PlayerController.Instance.transform.position, new Color(1f, 0.3f, 0.3f), 26);

        SaveIfBetter();
        lastRunStones = StonesEarned;
        UIManager.Instance.ShowGameOver(Mathf.FloorToInt(distance), CurrentRealm, score, bestDistance,
            RealmNames[Mathf.Min(bestRealm, RealmNames.Length - 1)], lastRunStones, MetaSystem.Stones, reviveCount < 3);
    }

    // 仅在真正退出时兜底结算。注意：切后台（编辑器失焦 / 网页切标签）绝不能结算，
    // 否则 runCommitted 会被提前置位，导致死亡面板的复活 / 双倍按钮全部失效。
    void OnApplicationQuit()
    {
        if (state == GameState.GameOver) CommitRun(1);
    }

    void LoadSave()
    {
        bestDistance = PlayerPrefs.GetInt(KeyDistance, 0);
        bestRealm = PlayerPrefs.GetInt(KeyRealm, 0);
    }

    void SaveIfBetter()
    {
        bool changed = false;
        int d = Mathf.FloorToInt(distance);
        if (d > bestDistance) { bestDistance = d; PlayerPrefs.SetInt(KeyDistance, bestDistance); changed = true; }
        if (realmIndex > bestRealm) { bestRealm = realmIndex; PlayerPrefs.SetInt(KeyRealm, bestRealm); changed = true; }
        if (changed) PlayerPrefs.Save();
    }
}
