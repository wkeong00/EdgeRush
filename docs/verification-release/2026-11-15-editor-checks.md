# 发布配置保护验收 · 2026-11-15

日期按 Asia/Singapore。此记录仅覆盖本轮发布配置与编辑器工具；2026-10-28 的 A/B 92 项／35 张图为另一份历史记录，本轮没有重跑全部游戏流程。

## 环境与执行

- Windows，Unity Editor **6000.6.7f1**。
- 用户要求使用 Unity 插件并批准更新工具；Unity CLI **1.0.0**，项目控制包 `com.unity.pipeline` **1.0.0**，已锁定 manifest／lockfile。
- `unity status` 返回本项目主编辑器 **ready**；所有命令显式指定本项目路径，未借用其他项目。
- 使用 Unity 插件 `script recompile`／`script recompile_status`：`completed`，`failed:false`，`errors:[]`，`compilationFailed:false`。
- 通过 `script eval` 调用 `ReleaseSetupChecks.Run()` 与 `ReleaseBuildRegressionChecks.Run()`，返回：

```json
{"passed":true,"setupAssertions":70,"guardAssertions":13}
```

## 已验证的行为

70 项设置检查覆盖连续 Apply 时保留身份、版本码、自定义签名引用／凭据、显式 target SDK，min SDK 只上调；正常应用竖屏／IL2CPP／ARM64／game 类别；Main 追加注册，保留其他场景的顺序、启用状态和已有禁用 Main；完整恢复原 appCategory，包括 `audio` 类别。

13 项独立 Release 检查使用虚构快照，覆盖 Android 非 Development 作用范围、占位／空身份、包名／版本不一致、非正 versionCode、不安全／占位隐私地址及缺失签名字段。校验先于旧广告校验运行，不受广告开关影响。

设置检查在 finally 中恢复原值，清理临时场景文件及 EditorPrefs。复核 `EditorBuildSettings.asset` 与 `EditorSettings.asset` 无本轮差异，未留下 `Assets/__ReleaseSetupChecks_*`。Unity 对原本空的 keystore 引用曾序列化为 `{inproject}: `，API 读取仍为空；安全关闭编辑器后已恢复原空字段格式，未创建或配置真实密钥。

本轮首次批处理及首次 live 检查曾因“未保存 Untitled 场景时禁止 additive NewScene”失败，未记为通过。现已改为复制已有 Main 作为临时探针，不打开／替换用户场景；自动生成 Main 对该状态给出明确错误，保留 Untitled。修复后重新编译并取得上方通过结果。

## 实际工程复核

```json
{
  "company": "Gizmobyte",
  "packageName": "com.gizmobyte.edgerush",
  "version": "0.1.0",
  "versionCode": 1,
  "actualReleaseIssues": [
    "privacy_url_invalid",
    "custom_signing_disabled",
    "keystore_path_missing",
    "keystore_alias_missing"
  ],
  "openSceneCount": 1
}
```

包名是首次上传前待核对的拟定值。当前正式构建应被阻止：政策网址还是占位值，发布签名未准备；旧广告校验也尚待最终网络确定后改写。guard 的通过用例只证明规则工作，不代表实际工程已具备发布资格。

随后通过插件在用户既有组织创建／关联 EdgeRush Cloud 项目；正常关闭检查返回 `unsaved:[]`，重新打开后为 ready。插件读取 `Application.cloudProjectId` 与远端新项目一致，厂商／包名保留，四项实际 Release 问题不变；未启用广告、付费构建或收款服务。

## 尚未验证

- Android APK／AAB 构建、安装、签名指纹、原生 SDK 依赖和设备兼容。
- 真实广告／隐私同意、平台审核、正式投放、收入或实际收款。
- 新接广告 SDK；本轮只安装 Unity 开发控制包，未安装广告 SDK。
- TapTap 后台账号权限、正式条目及最终包名可用性。

原始 Unity 诊断日志留在被忽略的 `Temp/`，未上传包含机器环境细节的完整日志；本记录只保留必要的公开配置和测试结论。签名字段快照仅在编辑器内存中用于比较／恢复，不输出或上传密码、令牌、银行信息。
