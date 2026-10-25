# Edge Rush — 项目说明

一款竖屏 3 车道「擦边躲避」超休闲游戏，目标平台 TapTap 国际版（Android），靠激励视频广告变现。

本项目是**可运行的 Unity 工程**，代码、界面、美术、音效全部在运行时由 C# 生成——没有外部素材、没有需要手工摆放的场景、没有 prefab。

---

## 目录

| 路径 | 内容 |
| --- | --- |
| `EdgeRush/` | Unity 工程（用 Unity Hub 打开这个文件夹） |
| `EdgeRush/Assets/Scripts/` | 全部游戏代码 |
| `EdgeRush/Assets/Editor/EdgeRushSetup.cs` | 自动配置工程 + 生成启动场景 |
| `EdgeRush/Assets/Resources/Shaders/` | 两个极简 sprite 着色器（普通 / 叠加发光） |
| `EdgeRush/docs/` | 上手、打包、广告、上架文档 |
| `_tools/compilecheck.ps1` | 不开 Unity 也能做的离线编译检查（我用来验证代码） |

## 30 秒上手

1. Unity Hub → **Add** → 选择 `EdgeRush` 文件夹 → 用 **6000.6.7f1**（或任意 Unity 6 LTS）打开。
2. 等首次导入完成（会自动生成 `Assets/Scenes/Main.unity` 并写好打包设置）。
3. 按 **Play**。就这样——游戏自己会建好。

> 详细步骤和排错见 `docs/01-本地运行.md`。

## 美术预览

下面几张图是**离线渲染的程序化美术预览**（我用 Python 把 `Art.cs` 的 SDF 绘制逻辑原样复刻后渲染），不是游戏截图，但车辆、路障、金币、路面就是运行时画出来的东西：

- `docs/art-preview-scene.png` — 玩法画面示意（3 车道 / 玩家车 / 障碍车 / 金币 / 路障 / 路灯 / 霓虹路缘）
- `docs/art-preview-sprites.png` — 车辆与道具特写
- `docs/art-preview-car.png` — 玩家车放大

## 技术要点

- **引擎**：Unity 6（6000.6.7f1），内置渲染管线（Built-in），不需要 URP。
- **渲染**：正交相机 + SpriteRenderer + 少量 Mesh 四边形（滚动路面/速度线用贴图 UV 偏移，几乎零开销）。
- **美术**：`TexCanvas`（SDF 软件光栅器）在启动时画出所有车辆、路障、金币、图标、路面、背景、暗角。约 40 张图，全部程序生成。
- **音频**：`Synth` 合成 4 层音乐 stem（贝斯/琶音/主旋律/打击）与 9 个音效，无音频文件。可覆盖：把同名 clip 放进 `Assets/Resources/Audio/` 即优先使用。
- **数据**：`SaveSystem` 存 JSON + 校验 + 轻量加密，存 `persistentDataPath`，并备份到 PlayerPrefs。
- **广告**：`AdManager` + `IAdProvider`。编辑器里用 Mock 广告（3 秒倒计时），发布版切 AdMob（见 `docs/03-广告接入.md`）。
- **存档兼容**：`SaveData.version`，字段只增不改。

## 已实现的系统

核心手感 · 擦边连击 · 难度曲线 · 可解性检测 · 3 个场景主题 · 6+1 款车 · 金币经济 · 每日签到 · 每日任务 · 最高分 · 4 个广告位 · 设置/隐私 · 本地统计事件 · 程序化霓虹美术与音乐。

细节见 `docs/05-已完成与待办.md`。
