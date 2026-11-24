# Edge Rush — 项目说明

一款竖屏 3 车道「擦边躲避」超休闲游戏，目标平台 TapTap 国际版（Android），靠激励视频广告变现。

**新电脑／新 agent 先读：[项目现状与跨电脑交接](docs/11-项目现状与跨电脑交接.md)。** 2026-11-22 用户确认密钥已独立备份，原文件密码／证书核验和Unity签名配置已完成，外部副本恢复与正式APK仍待。[签名核验记录](docs/verification-release/2026-11-22-signing-setup.md)保存公开证据。签名文件和含密码私密副本不会随 GitHub clone 迁移。

当前按 [上线与盈利执行清单](docs/07-TapTap国际版上线与广告盈利执行清单.md) 推进。用户仍选择先只发TapTap；[Unity工单00945668](docs/10-Unity广告接入支持询问.md)明确该分发方式不支持Unity Ads／LevelPlay正式广告收入，旧直连候选已失效。[Mintegral资格询问](docs/12-Mintegral资格核实与询问草稿.md)已获用户单独授权并发送，等待适用答复；尚未取得替代资格、接入真实广告或发布。开发者Thingamajig，厂商Gizmobyte；拟定包名`com.gizmobyte.edgerush`首次上传前仍待核对。

2026-11-25：最新ARM64 Development APK已在BlueStacks安装并完成基础玩法、Mock奖励与重开存档[初测](docs/verification-release/2026-11-25-android-emulator-smoke.md)；真机、正式签名包、升级和真实广告仍待。TapTap后台已核对Gizmobyte厂商ID33864694及空列表，首个游戏的基础表单已填但图标上传受阻，**尚未保存或取得游戏ID**，见[后台记录](docs/verification-release/2026-11-25-taptap-preflight.md)。Unity收款档案已Verified；[英文商店资料](docs/09-TapTap上架资料草稿.md)与[免费网站](https://wkeong00.github.io/EdgeRush/)可沿用。开发包不能作为正式发布包。

本项目是**可运行的 Unity 工程**，游戏界面、美术、音效在运行时由 C# 生成，没有需要手工摆放的场景或 prefab。应用图标及商店品牌图另由仓库程序绘图生成，源码和PNG／SVG见 [品牌素材](docs/store-assets/README.md)。

---

## 目录

| 路径 | 内容 |
| --- | --- |
| `EdgeRush/` | Unity 工程（用 Unity Hub 打开这个文件夹） |
| `EdgeRush/Assets/Scripts/` | 全部游戏代码 |
| `EdgeRush/Assets/Editor/EdgeRushSetup.cs` | 自动配置工程 + 生成启动场景 |
| `EdgeRush/Assets/Resources/Shaders/` | 两个极简 sprite 着色器（普通 / 叠加发光） |
| `EdgeRush/docs/` | 上手、打包、广告、上架文档 |
| `tools/compilecheck.ps1` | 已纳入仓库的离线编译检查；用法与限制见 `tools/README.md` |
| `Assets/Editor/ReleaseSetupChecks.cs` | 发布设置保留检查；独立 Release 身份／政策／签名校验见 `ReleaseBuildValidation.cs` |

## 30 秒上手

1. Unity Hub → **Add** → 选择 `EdgeRush` 文件夹 → 用锁定的 **6000.6.7f1** 打开。
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
- **编辑器控制**：Unity CLI 1.0.1／Pipeline 1.0.0；用于本地开发与检查，未启用云付费构建。正式构建身份、版本与签名单独维护，Apply 不再重置它们。
- **渲染**：正交相机 + SpriteRenderer + 少量 Mesh 四边形（滚动路面/速度线用贴图 UV 偏移，几乎零开销）。
- **美术**：`TexCanvas`（SDF 软件光栅器）在启动时画出所有车辆、路障、金币、图标、路面、背景、暗角。约 40 张图，全部程序生成。
- **音频**：`Synth` 合成 4 层音乐 stem（贝斯/琶音/主旋律/打击）与 9 个音效，无音频文件。可覆盖：把同名 clip 放进 `Assets/Resources/Audio/` 即优先使用。
- **数据**：`SaveSystem` 存 JSON + 校验 + 轻量加密，存 `persistentDataPath`，并备份到 PlayerPrefs。
- **广告**：`AdManager` + `IAdProvider`。Editor／Development 用 Mock；Release 缺真实 SDK／正式配置会阻止构建。最终广告网络与渠道仍需确认，TapTap-only 不能默认完整接入 AdMob 变现（见 `docs/03-广告接入.md`）。
- **存档兼容**：`SaveData.version`，字段只增不改。

## 已实现的系统

核心手感 · 擦边连击 · 难度曲线 · 可解性检测 · 3 个场景主题 · 6+1 款车 · 金币经济 · 每日签到 · 每日任务 · 最高分 · 4 个广告位 · 设置/隐私 · 本地统计事件 · 程序化霓虹美术与音乐。

本轮已实现 v2.0 的 A/B 批次：奖励幂等与存档迁移、七日签到循环、自适应界面、操作教学和触摸清理。Unity 6000.6.7f1 已跑通检查流程并生成 35 张界面验收图；Android 真机和真实 SDK 尚未验收。细节见 `docs/06-A-B-实施与验收.md` 与 `docs/05-已完成与待办.md`。
