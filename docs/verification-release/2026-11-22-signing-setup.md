# 发布签名核验与 Unity 配置 · 2026-11-22

日期按 Asia/Singapore。本记录覆盖现有密钥的只读核验、用户备份确认和 Unity 签名设置，不代表正式 APK、广告资格或商店发布通过。

## 1. 核验的现有文件

| 项目 | 结果 |
| --- | --- |
| 相对路径 | `UserSettings/Signing/edgerush-release.keystore` |
| 大小 | 4,300 bytes |
| 加密文件 SHA-256 | `50E757FBD4A4C6A502CE83A62C2D9D239A2C0F86BDCE8923E291E54E200D6FE3` |
| Alias | `edgerush` |
| 公开证书 SHA-256 | `C8:A9:AD:10:0D:03:29:D1:E8:36:0A:26:FF:18:1F:65:AC:51:C9:FB:31:7E:86:22:0A:19:0D:19:06:B3:C3:EA` |
| 公钥／签名算法 | 4096-bit RSA／SHA256withRSA |
| 证书有效期 | 2026-11-22 02:01:17 SGT 至 2054-04-09 02:01:17 SGT |
| 密码／别名核验 | 用户此前指定记录的值可通过 keytool 解锁并读取该 alias 的公开证书 |
| 文件是否改变 | 核验前后 SHA-256 一致，未生成或替换密钥 |
| Git 状态 | 密钥位于被忽略的 UserSettings，不提交 |

keytool 使用锁定 Editor 的 OpenJDK；密码只放在该子进程的随机命名环境变量，通过 `-storepass:env` 引用。未放在 keytool 参数、公开输出或本记录中，进程退出后清理引用。

用户本轮明确确认**已经备份到本机以外**。这是用户确认的独立备份状态；本轮没有读取该外部副本、核对其 hash 或做实际恢复演练。不能把原文件可解锁扩大成外部备份已验证。

## 2. 工具、启动与还原点

- 沿用用户此前允许更新 CLI 的授权，从 1.0.0 更新到 **1.0.1**，安装结果和再次读取版本均确认成功。
- Editor 仍为 **6000.6.7f1**，项目 Pipeline 仍为 **1.0.0**；未升级 Editor／包。
- 本机起初没有 Unity Editor 进程。CLI 打开原工程时报告 `OPEN_BLOCKED_BY_DIALOG`，标题 `Scene Backup Detected`，按钮 Keep Backups／Delete。
- 随后通过电脑操作技能选择返回的 Unity 窗口并观察：提示已消失，主窗口已打开。agent 没有执行该提示的按钮操作，不能声称由 agent 点击 Keep Backups。
- CLI 再次读取本项目为 ready。Editor 启动后出现未跟踪的 `Assets/_Recovery/0.unity` 及对应 meta，保留，不删除或混入签名提交。
- 实际设置前保留配置副本：`UserSettings/SigningSetupRestore/2026-11-22/ProjectSettings.before-signing.asset`。
- 原配置与还原副本 SHA-256 均为 `21458EF864D9B1E477F158BB2F98D7878354DD57FDE29EF975526E71CF7DDD0B`。副本只供本机恢复，不上传。

## 3. 通过 live Unity 插件设置并回读

```json
{
  "customKeystore": true,
  "keystore": "UserSettings/Signing/edgerush-release.keystore",
  "keystoreExists": true,
  "alias": "edgerush",
  "storePasswordAvailable": true,
  "keyPasswordAvailable": true,
  "releaseIdentityIssues": [],
  "playing": false,
  "compiling": false
}
```

`ProjectSettings/ProjectSettings.asset` 的有效改动是：

- `AndroidKeystoreName: '{inproject}: UserSettings/Signing/edgerush-release.keystore'`
- `AndroidKeyaliasName: edgerush`
- `androidUseCustomKeystore: 1`

Unity 另产生一处空 YAML 值的尾随空白序列化差异；未改变图标内容。公司、产品、版本、versionCode 和包名回读仍分别为 Gizmobyte、Edge Rush、0.1.0、1、com.gizmobyte.edgerush。

密码从用户指定的忽略文件在本机读取后赋给本次 Editor 会话，不在 CLI 的代码参数内写明文密码。回读只返回“是否可用”布尔值，不返回密码；保存后明确检查 ProjectSettings 不含用户提供的密码。新机器／新会话仍需按实际 Editor 状态提供凭据，不能假定 clone 后自动恢复。

`ReleaseBuildValidation.Validate(CaptureCurrent())` 返回空问题列表。该校验只证明身份／版本／政策格式／签名引用满足现有规则；**不证明真实广告保护通过或正式 APK 已可发布**。

## 4. Console 与边界

- 设置前后的 Console 均为 **0 errors／1 existing warning**，未清空用户 Console。
- 同一 Console session，cursor 1；`compilationFailed:false`，`compiling:false`。
- 没有修改玩法 C#、安装广告 SDK、执行构建、签 APK、安装运行或发布。
- 当前真实 Provider／生产 ID 未接通，且 Unity 支持工单00945668已明确当前 TapTap-only 条件不支持正式广告收入；不能为了构建关掉现有保护。
- 最终 Release 仍需核验实际证书、Mock 排除、依赖／权限、ABI／API／16KB、真机与覆盖升级。
- 当前 Debug APK 与正式签名包同包名异证书，不能直接覆盖安装；覆盖升级应使用同一正式密钥签出的前后版本并递增 versionCode。

对应 Unity 6000.6 文档：[加载现有 keystore](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/android/getting-started/keystore/load)、[keystorePass](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/playersettings/android/keystorepass)、[keyaliasPass](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityeditor/playersettings/android/keyaliaspass)。本记录不保存私钥或密码。
