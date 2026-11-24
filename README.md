# 道途：无尽飞升 (Dao: Endless Ascension)

基于 Unity 2022.3 LTS 开发的修仙题材 2D 跑酷 Roguelike 游戏。玩家扮演穿越者，在仙道上御剑狂奔，躲避心魔与禁制，吸收灵气突破境界，通过三选一奇遇组合出自己的流派。

> 本项目面向「游戏系统策划」岗位设计。在可玩的 Demo 之外，按 JD 补齐了养成、任务、成就、活跃、道具、关卡、商城、广告变现、内购付费等系统模块，并给出完整数值与资源循环设计。
>
> **完整系统策划案：[GAME_DESIGN.md](GAME_DESIGN.md)**

## 在线试玩

[点击这里直接玩网页版（GitHub Pages）](https://tyuiop246-tr.github.io/Xiuxian/)

> 打开即可游玩，建议使用电脑键盘操作。若长时间未加载，请按 `Ctrl + F5` 强制刷新。

## 系统设计一览（对应岗位 JD）

| JD 模块 | 本 Demo 的落地实现 | 代码 / 文档 |
|---|---|---|
| 养成 | 修仙洞府：道心根基 / 御剑术 / 顿悟道痕三条线，各 5 级 | `MetaSystem.Upgrade` |
| 任务 | 每日任务：跑酷 2000 米 / 吸收 50 灵气 / 完成 3 局，按本地日期重置 | `MetaSystem.ClaimQuest` |
| 成就 | 5 个长期成就，达成后可领取灵石 | `MetaSystem.ClaimAchievement` |
| 活跃 | 七日签到，连续递增奖励、断签重置，第 4 天产出复活符 | `MetaSystem.SignToday` |
| 道具 | 复活符（签到产出，用于广告复活） | `MetaSystem.UseRevive` |
| 社交 | 好友助力 / 排行榜方案 | `GAME_DESIGN.md` |
| 关卡 | 10 重境界 + 每 300 米「三选一」奇遇 | `GameManager.TriggerEncounter` |
| 商城 | 复活符、灵石礼包定价与限购 | `GAME_DESIGN.md` |
| 广告变现 | 复活广告（+3 秒无敌）、双倍结算广告 | `GameManager.WatchAdRevive` / `WatchAdDouble` |
| 内购付费 | 去广告、月卡、灵石礼包 | `GAME_DESIGN.md` |

## 完整资源循环

```
局内跑酷（距离 + 灵气）
      │  结算
      ▼
    灵石 ──┬──> 修仙洞府升级 ──> 初始护体 / 移速 / 开局功法概率提升 ──┐
           ├──> 每日任务 / 成就 / 签到领奖 ──> 灵石 + 复活符          │
           └──> 复活符 ──> 广告复活 ──> 继续跑图 ──> 更高收益 ───────┘
```

关键设计：**结算幂等性**。广告复活期间不结算，只有真正结束本局时才通过 `CommitRun()` 统一发奖与记录任务进度，避免复活后重复发奖 / 重复计入任务；切后台或关闭页面时兜底提交，防止进度丢失。

## 技术栈

- **引擎**: Unity 2022.3 LTS
- **语言**: C#
- **核心架构**: 数据驱动 (ScriptableObject)、对象池 (Object Pooling)、状态机 (GameManager 单例)
- **局外系统**: MetaSystem 静态数据层 + PlayerPrefs 存档（灵石、养成等级、任务、成就、签到）
- **UI**: IMGUI / OnGUI（主菜单、HUD、奇遇三选一、结算、洞府、任务成就面板）
- **资源**: 像素风角色与背景（Point 过滤、运行时重建 Sprite）；WebGL 内置中文字库

## 核心功能

**核心玩法**

- **基础跑酷**: 基于 Rigidbody2D 的左右移动、跳跃、冲刺；角色按方向自动转身
- **Roguelike 构筑**: 12 种可叠加功法（身法 / 护体 / 攻伐 / 神通），每 300 米触发「三选一」奇遇
- **境界系统**: 收集灵气突破炼气 → 筑基 → 金丹 → … → 仙人共 10 重境界
- **手感反馈**: 屏幕震动、粒子爆散、无缝滚动像素背景

**系统策划模块**

- **修仙洞府**: 三条养成线各 5 级，灵石消耗随等级递增，直接作用于下一局开局属性
- **任务 / 成就 / 七日签到**: 每日重置、进度追踪、一键领取，构成稳定的灵石产出与回访动机
- **复活符道具**: 签到产出，支撑「道心破碎」后的广告复活场景
- **广告变现**: 复活广告（原地复活 + 3 秒无敌）、双倍结算广告，且有单局次数上限
- **存档与兜底**: PlayerPrefs 持久化全部局外数据，结算幂等 + 切后台兜底提交

**性能与工程**

- 对象池复用障碍物与灵气，避免高频 Instantiate/Destroy 造成 GC 卡顿
- WebGL 命令行静默构建脚本，产物可直接部署到 GitHub Pages
- 中文 UI 字体裁剪内置，解决 WebGL 无法读取系统字体导致的乱码 / 缺字

## 操作说明

| 按键 | 功能 |
|---|---|
| `Space` / `Enter` | 在主菜单开始游戏；局内跳跃 |
| `A / D` 或 `← / →` | 左右移动（角色自动转身） |
| `W` | 跳跃 |
| `Left Shift` | 冲刺（需获得「瞬影遁」功法） |
| `C` | 切换男 / 女白衣修士 |
| `1 / 2 / 3` | 奇遇时选择功法 |
| `R` | 道心破碎后重新开始 |

主菜单可点击「修仙洞府」进行养成、「任务成就」领取每日任务与成就奖励。

## 项目结构

```
Assets/
├── Scripts/
│   ├── GameManager.cs       # 状态机、境界、奇遇、结算与广告变现
│   ├── MetaSystem.cs        # 局外资源循环：灵石/养成/任务/成就/签到
│   ├── PlayerController.cs  # 角色控制与功法效果
│   ├── Spawner.cs           # 对象池驱动的生成逻辑
│   ├── ObjectPool.cs        # 对象池
│   ├── TechniqueData.cs     # 功法 ScriptableObject 定义
│   ├── UIManager.cs         # OnGUI 界面系统（含洞府、任务成就面板）
│   ├── PixelBackground.cs   # 无缝滚动背景
│   └── PixelArtLoader.cs    # 像素图加载
├── Resources/
│   ├── Art/                 # 像素角色与背景
│   ├── Fonts/DaoFont.ttf    # 内置中文字库（WebGL 中文显示）
│   └── Techniques/          # 12 个功法资产
├── Editor/
│   └── WebGLBuilder.cs      # WebGL 静默构建脚本
└── Scenes/SampleScene.unity

docs/                        # GitHub Pages 部署目录（无压缩 WebGL 产物）
GAME_DESIGN.md               # 完整系统策划案（JD 对照明细 / 数值 / 埋点 / 迭代路线）
```
