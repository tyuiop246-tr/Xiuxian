using System;
using System.Collections.Generic;
using UnityEngine;

// 局外养成与资源循环系统（存档、灵石、洞府、任务、成就、复活符）
// 对标 JD：养成 / 任务 / 成就 / 活跃 / 道具 / 商城 / 广告变现
public static class MetaSystem
{
    const string KStone = "Meta_Stone";
    const string KHeart = "Meta_HeartLv";
    const string KSpeed = "Meta_SpeedLv";
    const string KLuck = "Meta_LuckLv";
    const string KRunes = "Meta_Revives";
    const string KTotalRuns = "Meta_TotalRuns";
    const string KMaxRealm = "Meta_MaxRealm";
    const string KBestDist = "Meta_BestDist";
    const string KTotalTechniques = "Meta_TotalTech";
    const string KLastDate = "Meta_LastDate";
    const string KSignDays = "Meta_SignDays";

    // 每日任务进度
    const string KQuestDist = "Meta_QDist";
    const string KQuestQi = "Meta_QQi";
    const string KQuestRuns = "Meta_QRuns";
    const string KQuestClaimed = "Meta_QClaimed";

    // 成就领取标记
    const string KAchBase = "Meta_Ach_";

    public static int Stones => PlayerPrefs.GetInt(KStone, 0);
    public static int HeartLv => PlayerPrefs.GetInt(KHeart, 0);
    public static int SpeedLv => PlayerPrefs.GetInt(KSpeed, 0);
    public static int LuckLv => PlayerPrefs.GetInt(KLuck, 0);
    public static int Revives => PlayerPrefs.GetInt(KRunes, 1);
    public static int TotalRuns => PlayerPrefs.GetInt(KTotalRuns, 0);
    public static int MaxRealm => PlayerPrefs.GetInt(KMaxRealm, 0);
    public static int BestDist => PlayerPrefs.GetInt(KBestDist, 0);
    public static int TotalTechniques => PlayerPrefs.GetInt(KTotalTechniques, 0);

    public static int[] HeartCosts = { 50, 100, 200, 400, 800 };
    public static int[] SpeedCosts = { 40, 80, 160, 320, 640 };
    public static int[] LuckCosts = { 60, 120, 240, 480, 960 };

    public static void AddStones(int amount)
    {
        PlayerPrefs.SetInt(KStone, Mathf.Max(0, Stones + amount));
        PlayerPrefs.Save();
    }

    // 洞府升级：返回是否成功
    public static bool Upgrade(int type)
    {
        int lv, maxLv;
        int[] costs;
        string key;
        switch (type)
        {
            case 0: lv = HeartLv; costs = HeartCosts; key = KHeart; break;
            case 1: lv = SpeedLv; costs = SpeedCosts; key = KSpeed; break;
            case 2: lv = LuckLv; costs = LuckCosts; key = KLuck; break;
            default: return false;
        }
        maxLv = costs.Length;
        if (lv >= maxLv || Stones < costs[lv]) return false;
        PlayerPrefs.SetInt(KStone, Stones - costs[lv]);
        PlayerPrefs.SetInt(key, lv + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static int UpgradeCost(int type)
    {
        int lv = type == 0 ? HeartLv : type == 1 ? SpeedLv : LuckLv;
        int[] costs = type == 0 ? HeartCosts : type == 1 ? SpeedCosts : LuckCosts;
        return lv >= costs.Length ? -1 : costs[lv];
    }

    // 洞府加成应用到角色
    public static int StartShields => HeartLv;            // 初始护体
    public static float StartSpeedBonus => SpeedLv * 0.5f; // 初始移速加成

    public static bool TryRollStartTechnique()
    {
        return UnityEngine.Random.value < LuckLv * 0.1f;
    }

    public static void UseRevive()
    {
        if (Revives <= 0) return;
        PlayerPrefs.SetInt(KRunes, Revives - 1);
        PlayerPrefs.Save();
    }

    // 每日任务：按本地日期自动重置
    static string Today => DateTime.Now.ToString("yyyy-MM-dd");

    static void EnsureNewDay()
    {
        if (PlayerPrefs.GetString(KLastDate, "") != Today)
        {
            PlayerPrefs.SetString(KLastDate, Today);
            PlayerPrefs.SetInt(KQuestDist, 0);
            PlayerPrefs.SetInt(KQuestQi, 0);
            PlayerPrefs.SetInt(KQuestRuns, 0);
            PlayerPrefs.SetInt(KQuestClaimed, 0);
            PlayerPrefs.Save();
        }
    }

    public static void RecordRun(int distance, int qi, int realmIndex, int techniquesGain)
    {
        EnsureNewDay();
        PlayerPrefs.SetInt(KQuestDist, PlayerPrefs.GetInt(KQuestDist, 0) + distance);
        PlayerPrefs.SetInt(KQuestQi, PlayerPrefs.GetInt(KQuestQi, 0) + qi);
        PlayerPrefs.SetInt(KQuestRuns, PlayerPrefs.GetInt(KQuestRuns, 0) + 1);
        PlayerPrefs.SetInt(KTotalRuns, TotalRuns + 1);
        PlayerPrefs.SetInt(KMaxRealm, Mathf.Max(MaxRealm, realmIndex));
        PlayerPrefs.SetInt(KBestDist, Mathf.Max(BestDist, distance));
        PlayerPrefs.SetInt(KTotalTechniques, TotalTechniques + techniquesGain);
        PlayerPrefs.Save();
        CheckAchievements(realmIndex, distance);
    }

    public static int QuestProgress(int index)
    {
        EnsureNewDay();
        if (index == 0) return PlayerPrefs.GetInt(KQuestDist, 0);
        if (index == 1) return PlayerPrefs.GetInt(KQuestQi, 0);
        return PlayerPrefs.GetInt(KQuestRuns, 0);
    }

    public static int QuestGoal(int index) => index == 0 ? 2000 : index == 1 ? 50 : 3;
    public static int QuestReward(int index) => index == 0 ? 30 : index == 1 ? 20 : 10;

    public static bool QuestClaimable(int index) => QuestProgress(index) >= QuestGoal(index);

    public static bool ClaimQuest(int index)
    {
        EnsureNewDay();
        int mask = PlayerPrefs.GetInt(KQuestClaimed, 0);
        if ((mask & (1 << index)) != 0) return false;
        if (!QuestClaimable(index)) return false;
        PlayerPrefs.SetInt(KQuestClaimed, mask | (1 << index));
        AddStones(QuestReward(index));
        return true;
    }

    public static bool IsQuestClaimed(int index)
    {
        EnsureNewDay();
        return (PlayerPrefs.GetInt(KQuestClaimed, 0) & (1 << index)) != 0;
    }

    // 成就
    public static string[] AchievementNames = { "初入仙道", "金丹大成", "元婴出窍", "万里独行", "功法大典" };
    public static int[] AchievementRewards = { 20, 50, 100, 150, 200 };

    public static bool AchievementClaimable(int index)
    {
        switch (index)
        {
            case 0: return TotalRuns >= 1;
            case 1: return MaxRealm >= 2;
            case 2: return MaxRealm >= 3;
            case 3: return BestDist >= 5000;
            case 4: return TotalTechniques >= 12;
        }
        return false;
    }

    public static bool IsAchievementClaimed(int index) => PlayerPrefs.GetInt(KAchBase + index, 0) == 1;

    public static bool ClaimAchievement(int index)
    {
        if (!AchievementClaimable(index) || IsAchievementClaimed(index)) return false;
        PlayerPrefs.SetInt(KAchBase + index, 1);
        AddStones(AchievementRewards[index]);
        return true;
    }

    static void CheckAchievements(int realmIndex, int distance)
    {
        if (TotalRuns == 1) Debug.Log("[道途] 成就达成：初入仙道");
        if (realmIndex >= 2 && MaxRealm == realmIndex) Debug.Log("[道途] 成就达成：金丹大成");
        if (realmIndex >= 3 && MaxRealm == realmIndex) Debug.Log("[道途] 成就达成：元婴出窍");
        if (distance >= 5000) Debug.Log("[道途] 成就达成：万里独行");
        if (TotalTechniques >= 12) Debug.Log("[道途] 成就达成：功法大典");
    }

    // 活跃：七日签到
    public static int SignDays => PlayerPrefs.GetInt(KSignDays, 0);
    public static bool CanSignToday => PlayerPrefs.GetString("Meta_SignDate", "") != Today;
    public static int[] SignRewards = { 20, 30, 50, 0, 80, 100, 200 };

    public static int SignToday()
    {
        if (!CanSignToday) return -1;
        string last = PlayerPrefs.GetString("Meta_SignDate", "");
        int days = SignDays;
        if (last != Yesterday) days = 0; // 断签重置
        days = Mathf.Min(days + 1, 7);
        PlayerPrefs.SetInt(KSignDays, days);
        PlayerPrefs.SetString("Meta_SignDate", Today);
        int reward = SignRewards[days - 1];
        if (reward > 0) AddStones(reward);
        else { PlayerPrefs.SetInt(KRunes, Revives + 1); }
        PlayerPrefs.Save();
        return reward;
    }

    static string Yesterday => DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
}
