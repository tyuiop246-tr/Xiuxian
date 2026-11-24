using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Ready, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // 保留场景里已序列化的旧字段，保证兼容
    public bool isGameOver = false;
    public int score = 0;

    public GameState state = GameState.Ready;
    public float scrollSpeed = 10f;
    public float encounterInterval = 300f; // 奇遇间隔（米），Inspector 可调为 1000

    public static readonly string[] RealmNames =
    {
        "炼气期", "筑基期", "金丹期", "元婴期", "化神期",
        "炼虚期", "合体期", "大乘期", "渡劫期", "仙人"
    };

    public int realmIndex = 0;
    public int qi = 0;
    public float distance = 0f;

    public int bestDistance = 0;
    public int bestRealm = 0;

    private float nextEncounterDistance;
    private TechniqueLibrary library;

    const string KeyDistance = "Dao_BestDistance";
    const string KeyRealm = "Dao_BestRealm";

    public int QiNeeded => 10 + realmIndex * 8;
    public string CurrentRealm => RealmNames[Mathf.Min(realmIndex, RealmNames.Length - 1)];

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
        {
            StartGame();
        }

        if (state == GameState.GameOver && Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    void RefreshHUD()
    {
        if (UIManager.Instance == null) return;
        int shield = PlayerController.Instance != null ? PlayerController.Instance.Shields : 0;
        UIManager.Instance.UpdateHUD(CurrentRealm, qi, QiNeeded, Mathf.FloorToInt(distance), shield,
            PlayerController.Instance != null ? PlayerController.Instance.GetTechniqueSummary() : "");
    }

    public void StartGame()
    {
        if (state != GameState.Ready) return;
        state = GameState.Playing;
        isGameOver = false;
        Time.timeScale = 1f;
        UIManager.Instance.HideMainMenu();
        EffectsController.Instance.Shake(0.06f, 0.15f);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddScore()
    {
        AddQi(1);
    }

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
        TechniqueData[] choices = PickThree();
        UIManager.Instance.ShowEncounter(choices, OnTechniqueChosen);
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
            int idx;
            int guard = 0;
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
        Time.timeScale = 1f;
        state = GameState.Playing;
        UIManager.Instance.HideEncounter();
        UIManager.Instance.ShowToast("获得功法：" + tech.techniqueName);
        RefreshHUD();
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
        UIManager.Instance.ShowGameOver(Mathf.FloorToInt(distance), CurrentRealm, score, bestDistance,
            RealmNames[Mathf.Min(bestRealm, RealmNames.Length - 1)]);
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
