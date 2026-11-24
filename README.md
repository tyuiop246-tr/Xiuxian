# 道途：无尽飞升 (Dao: Endless Ascension)

基于 Unity 2022.3 LTS 开发的一款修仙题材 2D 跑酷 Roguelike 游戏。玩家扮演穿越者，在仙道上御剑狂奔，躲避心魔与禁制，吸收灵气突破境界，通过三选一奇遇组合出自己的无敌流派。

## 在线试玩

[点击这里直接玩网页版（GitHub Pages）](https://tyuiop246-tr.github.io/Xiuxian/)

> 打开即可游玩，建议使用电脑键盘操作。若长时间未加载，请按 `Ctrl + F5` 强制刷新。

## 技术栈

- **引擎**: Unity 2022.3 LTS
- **语言**: C#
- **核心架构**: 数据驱动 (ScriptableObject)、对象池 (Object Pooling)、状态机 (GameManager 单例)
- **UI**: IMGUI / OnGUI（主菜单、HUD、奇遇三选一、结算面板）
- **资源**: 像素风角色与背景（Point 过滤、运行时重建 Sprite）

## 核心功能

- **基础跑酷**: 基于 Rigidbody2D 的左右移动、跳跃、冲刺；角色按方向自动转身
- **Roguelike 构筑**: 12 种可叠加功法（身法 / 护体 / 攻伐 / 神通），每 300 米触发「三选一」奇遇
- **境界系统**: 收集灵气突破炼气 → 筑基 → 金丹 → … → 仙人共 10 重境界
- **道心系统**: 1 点生命，护体灵气与复活功法可提供容错
- **性能优化**: 对象池复用障碍物与灵气，避免高频 Instantiate/Destroy 造成 GC 卡顿
- **存档**: PlayerPrefs 保存历史最远距离与最高境界
- **手感反馈**: 屏幕震动、粒子爆散、无缝滚动像素背景
- **WebGL**: 已构建网页版，可部署到 itch.io 等平台公开试玩

## 操作说明

| 按键 | 功能 |
|---|---|
| `A / D` 或 `← / →` | 左右移动（角色自动转身） |
| `Space` / `W` | 跳跃；获得踏云步后可二段跳 |
| `Left Shift` | 冲刺（需获得「瞬影遁」功法） |
| `C` | 切换男 / 女白衣修士 |
| `1 / 2 / 3` | 奇遇时选择功法 |
| `R` | 道心破碎后重新开始 |

## 项目结构

```
Assets/
├── Scripts/                 # 核心 C# 代码
│   ├── GameManager.cs       # 状态机、境界、存档、奇遇触发
│   ├── PlayerController.cs  # 角色控制与功法效果
│   ├── Spawner.cs           # 对象池驱动的生成逻辑
│   ├── ObjectPool.cs        # 对象池
│   ├── TechniqueData.cs     # 功法 ScriptableObject 定义
│   ├── UIManager.cs         # OnGUI 界面系统
│   ├── PixelBackground.cs   # 无缝滚动背景
│   └── PixelArtLoader.cs    # 像素图加载
├── Resources/
│   ├── Art/                 # 像素角色与背景
│   └── Techniques/          # 12 个功法资产
├── Editor/
│   └── WebGLBuilder.cs      # WebGL 静默构建脚本
└── Scenes/SampleScene.unity
```
