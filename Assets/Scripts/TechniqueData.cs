using UnityEngine;

public enum TechniqueCategory
{
    身法,
    护体,
    攻伐,
    神通
}

public enum TechniqueEffect
{
    MoveSpeed,
    JumpForce,
    DoubleJump,
    Dash,
    Shield,
    Thorns,
    OrbitSword,
    Thunder,
    Magnet,
    InvincibleOnBreak,
    MaxLives,
    QiGain
}

[CreateAssetMenu(fileName = "Technique", menuName = "Dao/Technique")]
public class TechniqueData : ScriptableObject
{
    public string techniqueName;
    [TextArea] public string description;
    public TechniqueCategory category;
    public TechniqueEffect effect;
    public Color iconColor = Color.white;
    public float value = 1f;
    public float value2 = 0f;
    public int maxStack = 3;
}

// 功法库资源缺失时的运行时兜底，保证游戏永远可玩
public static class TechniqueFallback
{
    public static TechniqueData Create(int index)
    {
        var d = ScriptableObject.CreateInstance<TechniqueData>();
        var effects = new[] { TechniqueEffect.MoveSpeed, TechniqueEffect.Shield, TechniqueEffect.OrbitSword,
            TechniqueEffect.DoubleJump, TechniqueEffect.Thunder, TechniqueEffect.Magnet };
        var names = new[] { "风行术", "灵气护盾", "飞剑环绕", "踏云步", "天雷引", "摄灵诀" };
        var descs = new[] { "御剑速度提升", "获得一层护体灵气", "飞剑环绕护身", "可在空中再跳一次", "定期降下天雷清屏", "自动吸引周围灵气" };
        int i = Mathf.Abs(index) % effects.Length;
        d.techniqueName = names[i];
        d.description = descs[i];
        d.category = (TechniqueCategory)(i % 4);
        d.effect = effects[i];
        d.value = 1f;
        d.maxStack = 3;
        d.iconColor = Color.HSVToRGB(i / 6f, 0.6f, 1f);
        return d;
    }
}
