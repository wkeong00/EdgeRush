# 首个 Android Development APK 验收 · 2026-11-16

日期按 Asia/Singapore。本记录证明首次 Android 构建成功及其产物的静态检查结果；尚未证明安装后可玩。此 APK 是含 Mock 广告的私下测试包，使用 Android Debug 证书且允许调试，不作为正式发布候选。

## 构建记录

| 项目 | 实测值 |
| --- | --- |
| Unity Editor | 6000.6.7f1，Windows |
| Unity CLI / 控制包 | 1.0.0 / `com.unity.pipeline` 1.0.0 |
| 审计时源码 HEAD | `a801db6`；当时另有资料文档变更未提交 |
| 构建 ID | `build_3bb5ab072eb7` |
| 开始 / 结束 UTC | `2026-11-15T16:00:29.918469Z` / `2026-11-15T16:07:21.5642201Z` |
| 开始 / 结束新加坡时间 | 2026-11-16 00:00:29.918 / 00:07:21.564 |
| 结果 / 用时 | `Succeeded`，411645 ms，0 errors，17 warnings |
| 产物 | `Builds/Android/EdgeRush-0.1.0-dev.apk` |
| 文件实际大小 | **29318520 bytes**，约 27.96 MiB |
| APK SHA-256 | `F3DDD53435C9CB22DA15BDAB997F35BF0578DB558684761183016E5D9E94FF80` |

完整本地状态位于被忽略的 `Temp/pipeline_build_status.json`，核对了构建 ID 和时间；未把完整 files、packedAssets、buildSteps 或机器日志加入仓库。状态中的 `totalSizeBytes=375253290` 是 Unity 报告值，不是 APK 文件大小；下载大小以实际文件为准。

本轮仅读取产物与日志，新增这份记录及摘要 JSON；未重新构建、安装 APK、修改 PlayerSettings、读取正式签名文件或密码。

## APK 与签名实测

使用本机 Unity Android SDK 的 build-tools **36.0.0**：`aapt dump badging`、`aapt dump permissions`，以及 Unity OpenJDK 执行 `apksigner.jar verify --verbose --print-certs`。签名检查直接读取 APK 中的公开证书。

| 项目 | 实测值 |
| --- | --- |
| application label | `Edge Rush` |
| package | `com.gizmobyte.edgerush` |
| versionName / versionCode | `0.1.0` / `1` |
| minSdk / targetSdk / compileSdk | 26 / 36 / 36 |
| 原生 ABI | 仅 `arm64-v8a` |
| debuggable | **true** |
| 启动 Activity | `com.unity3d.player.UnityPlayerGameActivity` |
| 图形 / 方向要求 | OpenGL ES 3.1，portrait |
| 签名验证 | **Verifies**，1 个签名者，APK Signature Scheme **v2** 验证通过 |
| 证书 DN | `C=US, O=Android, CN=Android Debug` |
| 证书 SHA-256 | `e1ead17f5ccead2a2ede797217040f76c568472f5ac0fd979b2dd591e0ef3b32` |

清单请求的权限完整列表：

- `android.permission.INTERNET`
- `android.permission.VIBRATE`
- `com.gizmobyte.edgerush.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`（同时由本应用定义）

上述结果只适用于这一份哈希对应的 APK。没有正式发布证书验收；未来正式签名产物需要重新核对指纹、包名、版本、权限和哈希。

## 16 KB 内存页静态检查

2026-11-16 使用已安装的 NDK `llvm-readelf` **18.0.3** 和 SDK build-tools **36.0.0** 核对同一哈希的 APK，没有重新签名、改包或重构建。按 [Android 官方 16 KB 检查方法](https://developer.android.com/guide/practices/page-sizes) 检查每个原生库的 `LOAD` 段，并运行只读命令 `zipalign -v -c -P 16 4 <APK>`。

| APK 内 ARM64 原生库 | LOAD 段数 | 全部 LOAD 的 p_align | ELF 对齐结果 |
| --- | --- | --- | --- |
| `libc++_shared.so` | 4 | `0x4000`（16 KB） | 通过 |
| `libgame.so` | 3 | `0x4000`（16 KB） | 通过 |
| `libil2cpp.so` | 4 | `0x4000`（16 KB） | 通过 |
| `libmain.so` | 4 | `0x4000`（16 KB） | 通过 |
| `libswappywrapper.so` | 3 | `0x4000`（16 KB） | 通过 |
| `libunity.so` | 4 | `0x4000`（16 KB） | 通过 |

共 **6 个库、22 个 LOAD 段**，均达到官方要求的最小 `2**14` 对齐。额外核对 `GNU_RELRO`：6 个库的 RELRO 结束地址虽不是 16 KB 边界，但均与其所在 LOAD 段的结束地址相同，符合官方“同一 LOAD 段内没有后续可写数据”的允许条件。[RELRO 条件](https://developer.android.com/guide/practices/page-sizes)

`zipalign` 返回 **exit 0／Verification successful**。本 APK 的 6 个 `.so` 条目均为 **Deflate 压缩**，没有未压缩 `.so`；因此不能把这条结果表述成“6 个 ZIP 内原生库条目均已验证 16 KB 直接映射边界”。ELF 对齐已经独立检查；未来更改压缩方式或通过 AAB 分发时，需要重新核验最终分发 APK。[官方打包说明](https://developer.android.com/guide/practices/page-sizes)

检查前后 APK SHA-256 均为上表的 `F3DDD534…94FF80`。原始工具输出及逐库摘要仅留在忽略目录 `Temp/AndroidAlignment16K-88aspu_1/`，本页和摘要 JSON 记录必要结果。

**范围限制**：这只证明当前未接真实广告 SDK 的 Development APK 通过上述静态对齐检查；未在 16 KB 环境安装或运行，不证明游戏、内存分配或未来 SDK 的运行时兼容性。加入广告 SDK／原生依赖、改 Unity／NDK／打包配置、生成正式 APK 后，均须对新产物重做检查及设备验收。

## Pipeline 与广告代码边界

直接读取 APK 内 `assets/bin/Data/ScriptingAssemblies.json`，共 **89** 个程序集名称。其中唯一含 `Pipeline` 的名称是 `Unity.Pipeline.Attributes.dll`；`Unity.Pipeline.dll`、`Unity.Pipeline.IlInterpreter.dll`、`Unity.Pipeline.ScenePreview.dll` 均不存在。此 IL2CPP 产物未包含 Pipeline 主运行时程序集；不能声称控制包所有组成均为零字节。

这与 `Library/PackageCache/com.unity.pipeline@178b93997022/Runtime/Unity.Pipeline.asmdef` 的运行时启用条件、Attributes asmdef 无条件约束，以及当前空的自定义 scripting symbols 相符。`Library/Bee` 中有 Editor 编译缓存的 `Unity.Pipeline.dll`，不能据此判定它进入了 APK。正式构建或启用符号变更后仍需再检查实际产物。

APK 的 `global-metadata.dat` 中存在 `MockAdProvider`、`MockAdScreen` 类型名；结合当前 `Assets/Scripts/Systems/Ads.cs:128-145` 的 Provider 选择，符合本次 Development 包使用 Mock 的预期。尚未在设备上实际打开广告界面。`Packages/manifest.json` 中尚无真实广告 SDK；本次构建成功不能证明真实广告展示、收入或收款链路成立。

`ReleaseBuildValidation` 与 `AdBuildValidation` 对 Development 构建放行，因此本次成功也不证明政策地址、正式签名或生产广告配置已通过 Release 校验。

## 17 条构建警告分组

以本次 Pipeline BuildReport 的 `warnings` 数组计数，避免把日志中重复输出的同一条或其他工具提示重复计入。

| 警告 | 数量 | 影响与后续 |
| --- | --- | --- |
| `UAC0009`：`DEVELOPMENT_BUILD` 预处理指令已弃用 | 3 | 位于 `Assets/Scripts/Systems/Ads.cs:132,138,370`。本次未阻断构建；后续整理广告开发/正式路径时应按当前 managed code variant 及 `Debug.isDebugBuild` 语义处理，并重新验证正式产物排除 Mock。不能只机械替换为某个新宏。 |
| `UAC1001`：普通对象字段因缺少 `[Serializable]` 被跳过序列化 | 14 | `Assets/Scripts/UI/UiRoot.cs:18-29` 的 12 个界面/视图字段，以及 `Assets/Scripts/Core/GameApp.cs:28,43` 的 `Saver`、`LastSummary`。这些对象在运行时显式创建或赋值，本次代码审查未发现依赖 Inspector 反序列化的路径，当前判断为低风险；实际 Android 启动与界面仍待安装验证。 |

初始化证据：`UiRoot.cs:67-105` 创建界面/视图；`GameApp.cs:90` 创建 Saver，`GameApp.cs:576` 写入结算摘要。无需仅为消除警告把所有界面类标成可序列化。

Unity 对 managed code variant 与条件编译的当前说明：[Unity 6000.6 scripting symbols](https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/script-compilation/conditional-compilation/scripting-symbol-reference)。

另外，Gradle 日志含 `android.builtInKotlin=false`、`android.enableJetifier=true` 将在 AGP 10 移除的弃用提示，以及 SDK `package.xml` 只读元数据提示；最后仍为 `BUILD SUCCESSFUL`。Editor 日志另有 clang unused-argument、构建期间 heartbeat 延迟和云遥测超时。这些不在上方 17 条内，本轮没有发现它们阻断产物生成的证据；不应因此修改 Program Files 目录权限。

本地证据：`Logs/Editor.log`；`Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build/outputs/logs/unity-assembleDebug-build.log`（3-8 行为 Gradle 弃用提示，127 行为成功）。完整日志未提交。

## 待验证

- [ ] 安装并启动 APK；本次产物审计未安装，模拟器操作另行记录。
- [ ] 教程、滑动/释放、暂停恢复、死亡结算、重开及存档重启流程。
- [ ] Mock 成功/跳过/关闭/失败，以及奖励持久化；Mock 不产生广告收入。
- [ ] 真实 ARM64 Android 设备的触控、画面、安全区、性能、后台恢复与发热。
- [ ] 最终广告 SDK、隐私同意、真实测试广告和奖励回调乱序处理。
- [ ] 正式签名、非 Development APK、Release guards 和正式产物审计。

本记录不把 APK 构建成功、模拟器可启动、TapTap 审核、正式广告投放和收款视为同一验收结果。
