# Edge Rush · 仅 TapTap 国际版广告路线核实

> 核查日期：2026-11-15（Asia/Singapore）
>
> 状态：已完成公开规则初查，已在用户既有组织创建并关联 EdgeRush Cloud 项目；尚未取得本项目的广告平台投放／收款批准。未安装新广告 SDK，未注册新广告账户，未发送下方询问信。

## 1. 已确认的范围

- 发行：先只发 TapTap 国际版 `taptap.io`，暂不 Google Play。
- 主体：用户确认以马来西亚个人身份收款；实际付款资料、地址与银行方式需在官方后台核对。
- 公开名称：开发者 Thingamajig（用户提供 TapTap ID `799998484`），厂商 Gizmobyte。厂商显示名不代表收款主体变成注册公司。
- 支持邮箱：`wkeongg@gmail.com`，用户提供作公开支持用途。
- 游戏：先保留 Edge Rush；拟定包名 `com.gizmobyte.edgerush`。首次上传前检查包名／证书；尚未验证名称或包名可用性。
- 试玩：英文、成年玩家、先马来西亚小范围。商店与广告受众须反映实际产品和用户，不能仅靠这里的描述完成平台审核。
- AdMob：已注册，初始付款资料未完；账号保留，当前路线暂不继续其专属接入。

## 2. 当前选择：优先验证 Unity Ads 直连

Unity Ads 官方明确区分独立广告网络与聚合方案。项目设置文档说明 Store IDs 可选，跳过不阻止展示广告，但部分广告合作方需要这些信息；新后台也提供“尚无商店 URL”流程。因此 **Unity Ads 直连是有官方依据的候选**，无需先假定开发者必须开 Play Console。[流程](https://docs.unity.com/en-us/grow/ads/process-overview)、[Store IDs](https://docs.unity.com/en-us/grow/dashboard/get-started/project/settings#store-ids)、[添加应用](https://docs.unity.com/en-us/monetization/dashboard/add-an-app)。

仍需区分以下条件：

1. 官方 FAQ 对站外 Android 分发提示：广告收益还依赖玩家设备能够访问 Google Play。这是玩家设备条件，不是开发者先开 Play Console 的要求；TapTap 下载不代表设备自动满足它。[FAQ](https://docs.unity.com/en-us/grow/ads/references/faq)。
2. 截至核查，没有取得针对本项目 `taptap.io`-only、马来西亚个人收款的明确后台结论或官方答复。能添加应用和显示测试广告均不替代这一步。
3. 2026-04 起直连可能降低广告表现；官方仍表示继续支持并提供广告填充。首轮先评估是否能合规上线与收款，不据此承诺填充率或收入。[当前说明](https://docs.unity.com/en-us/monetization/getting-started/changelog)。
4. Unity 支持个人收款，但付款资料、身份／税务、可用方式和费用须按实际账户核对；不能套用 AdMob 的付款门槛。[FAQ](https://docs.unity.com/en-us/grow/ads/references/faq)、[付款概览](https://docs.unity.com/en-us/monetization/payments/intro-to-monetization-payments)、[付款资料](https://docs.unity.com/en-us/monetization/payments/complete-payout-profile)。

## 3. 候选比较与取舍

| 候选 | 可证明的事实 | 当前处理 |
| --- | --- | --- |
| AdMob | 受支持商店列表不含 TapTap；只在不受支持商店发布的应用无法完成 readiness 审核，广告受限 | 保留账号；不把它当作当前正式变现已解决。[应用设置](https://support.google.com/admob/answer/9989980?hl=en) |
| Unity Ads 直连 | 无 Store ID 不阻止展示；有无商店 URL 的添加流程 | 优先核实本项目正式广告与马来西亚个人付款资格 |
| LevelPlay／ironSource | 可以添加临时应用测试；新账户有人工批准流程，Live 流程关联 Play／App Store | 不把 Temp 或测试通过写成正式批准。[账户](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/create-account)、[应用](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/add-app) |
| AppLovin MAX | 官方说明替代商店并非正式支持范围，可手工添加但风险自行承担 | 不优先用于此次需要明确上架／收款路径的首轮验证。[官方说明](https://support.applovin.com/en/max/max-dashboard/ad-units/create-an-ad-unit) |
| Mintegral | 官方 API 支持未上商店、`other` 商店和 rewarded video | 可作第二候选；仍需确认 `.io` 流量、马来西亚个人与付款。[API](https://cdn-adn-https.rayjump.com/cdn-adn/open_api/MintegralAPI.html) |
| TopOn | 允许手工添加和测试；API 列 TapTap，但示例为 `.cn`；第三方源各自结算 | 聚合本身不保证广告需求或付款。ADX 需另谈协议，暂不先增加聚合层。[账号／结算](https://help.toponad.net/docs/MfO9IZ)、[API](https://help.toponad.net/docs/App-API-v3) |

Mintegral 和 TopOn ADX 的通用条款都存在低于 US$1,000 默认累积、低额申请由开发者承担银行费用等约定。它们不是简单的“完全不能低额提款”，但小流量项目可能较难达到经济上合适的首次收款；最终要看实际协议与费用。[Mintegral §5.2](https://dev.mintegral.com/document/term-en.pdf)、[TopOn §3.2.5](https://www.toponad.net/en/terms)。

## 4. 依次执行的核验门槛

- [x] 用户确认 TapTap-only、个人、马来西亚、公开开发者资料及首轮试玩范围。
- [x] 初查主要候选的商店限制，优先选择 Unity Ads 直连继续核实。
- [x] Unity CLI `auth status` 确认已有 Thingamajig 登录会话，邮箱与用户提供的公开邮箱一致；无需先重复注册。Unity Cloud 组织与 Monetization 权限单独核对。
- [x] Unity CLI `cloud org` 确认已有 Cloud 组织且账号角色为 Owner；没有新建组织，也没有修改权限。此角色不替代广告应用审核或收款验证。
- [x] 在该组织只读搜索名称含 Edge 的 Cloud 项目，返回空；没有创建新项目。这个结果不等于已查遍旧 Monetization 后台的所有项目。
- [x] 用户表示 Cloud 里尚无项目后，再列出该组织全部项目为空；通过 Unity 插件创建并关联 **EdgeRush**，远端复查返回该项目，ID `19ff5aec-d901-42b0-aa48-545cc9fcf640`。这不是广告 Game ID／广告位 ID。
- [x] 编辑器用 `--if-saved` 正常关闭（unsaved 为空），重新打开后为 ready；插件读取 `Application.cloudProjectId` 与新项目 ID 一致，厂商和拟定包名保留。
- [ ] 核对 Unity Monetization 后台现有项目、Owner 权限、启用广告的实际要求；若需要条款或身份资料，由用户处理对应步骤。
- [ ] 明确确认 `taptap.io`-only APK 的正式 rewarded 广告资格、缺 Store ID 的正确配置和审核材料。
- [ ] 明确马来西亚个人付款支持、实际付款方式、最低额、费用、周期、身份／税务要求；只记录状态，不保存银行资料。
- [ ] 固定正式包名与应用条目，创建 Game ID／所需 rewarded 广告位，强制开发设备使用测试模式。
- [ ] 改写执行清单 P4／P5／P10／P12 为实际 Unity Ads 流程；以该 SDK 的隐私要求替代 Google UMP 假设。
- [ ] 接入并锁定真实 SDK、替换 Provider 及发布配置校验，完成可靠奖励、隐私与 Android 真机检查。
- [ ] TapTap 公开下载并满足平台要求后，再向正常玩家开放正式广告，核验有效曝光、收入与实际付款。

没有公开条文明确回答的资格问题，先在官方后台核对，再由用户发送下方问题或授权我代发。当前不发送邮件／工单，不上传项目或账户资料。

Cloud 创建请求采用 `coppa:unspecified`；服务端复查实际返回 `coppa:not_compliant`、`kidsStoreCompliance:false`，数据收集／共享推荐值为 unspecified。这里如实保留字段差异，不把 Cloud 字段当作已经完成广告平台的成年／儿童受众、隐私或同意配置；进入 Monetization 后还要核对。

## 5. 给 Unity Ads 支持的问题草稿（未发送）

官方入口：[Unity Ads Support](https://support-ads.unity.com/s/)。

```text
Subject: Unity Ads eligibility for a TapTap International-only Android game

Hello Unity Ads Support,

I am an individual developer based in Malaysia. I plan to distribute a free
Android Unity game, Edge Rush, only on TapTap International (taptap.io).
The game will not have a Google Play or Apple App Store listing initially.
The initial audience is adults using English, with a small Malaysia launch.
I want to offer optional rewarded ads for revive and settlement bonuses.

Your documentation says linking Store IDs is optional and that skipping it
does not prevent ads from being shown. Before integration, please confirm:

1. Can this TapTap International-only APK be approved for paid rewarded ads
   using Unity Ads directly, without a Google Play listing?
2. What app review, public download URL, app-ads.txt or other ownership
   verification is required, and how should the missing Store ID be configured?
3. Are there player-device requirements, traffic thresholds or restrictions
   relevant to this distribution method?
4. Can a Malaysia-based individual receive payouts? Which payout methods,
   minimum thresholds, fees, payment schedule and verification/tax documents
   apply to this account?

Please distinguish test-ad access from approval for live monetization and payouts.
Thank you.
```

草稿未包含密码、证件、银行账户或真实广告凭据。取得答复后在主清单 C00 记录可核对结论；不将泛泛的“支持 Android”答复视为所有问题已解决。
