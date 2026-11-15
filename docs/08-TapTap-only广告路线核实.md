# Edge Rush · 仅 TapTap 国际版广告路线核实

> 核查日期：2026-11-15（Asia/Singapore）
>
> 状态：Cloud 项目已关联；用户已在 Monetization 添加 Android 应用并取得 Game ID。新版后台竞价／直连接法、正式投放与付款资格仍未完成核实。未安装新广告 SDK，未发送下方询问信。

## 1. 已确认的范围

- 发行：先只发 TapTap 国际版 `taptap.io`，暂不 Google Play。
- 主体：用户确认以马来西亚个人身份收款；实际付款资料、地址与银行方式需在官方后台核对。
- 公开名称：开发者 Thingamajig（用户提供 TapTap ID `799998484`），厂商 Gizmobyte。厂商显示名不代表收款主体变成注册公司。
- 支持邮箱：`wkeongg@gmail.com`，用户提供作公开支持用途。
- 游戏：先保留 Edge Rush；拟定包名 `com.gizmobyte.edgerush`。首次上传前检查包名／证书；尚未验证名称或包名可用性。
- 试玩：英文、先马来西亚成年测试者小范围；最新后台截图确认应用声明为普通大众、不针对儿童。这是目标受众声明，不是 18+ 分级；产品内容、宣传和实际受众仍须一致。
- AdMob：已注册，初始付款资料未完；账号保留，当前路线暂不继续其专属接入。
- Unity Monetization：用户已添加 **Edge Rush／Android**，关联项目 **EdgeRush**；详情截图显示 **Game ID `800438692`**，Store ID 为“未设置”。Game ID 不是 Cloud 项目 UUID、商店 ID 或广告位 ID。
- 激励广告位：用户已创建 **`BP_Rewarded_Android`**，完成弹窗显示 Placements created successfully，格式为 Rewarded；当前只见一个广告位。Stats API Key 显示 Not set，不保存任何统计密钥。尚未取得测试模式或实际广告请求证据。
- 付款状态：用户已在官方页面完成前端向导，最新截图到 **Done／You are all set**；背景仍提示 **Payouts are on hold／The tax forms have yet to be received**。需要关闭弹窗并刷新，核对最终税表接收／审核和暂扣状态；不把向导完成当作已可付款。

## 2. 当前选择：优先验证 Unity Ads 直连

Unity Ads 官方明确区分独立广告网络与聚合方案。项目设置文档说明 Store IDs 可选，跳过不阻止展示广告，但部分广告合作方需要这些信息；新后台也提供“尚无商店 URL”流程。因此 **Unity Ads 直连是有官方依据的候选**，无需先假定开发者必须开 Play Console。[流程](https://docs.unity.com/en-us/grow/ads/process-overview)、[Store IDs](https://docs.unity.com/en-us/grow/dashboard/get-started/project/settings#store-ids)、[添加应用](https://docs.unity.com/en-us/monetization/dashboard/add-an-app)。

仍需区分以下条件：

1. 官方 FAQ 对站外 Android 分发提示：广告收益还依赖玩家设备能够访问 Google Play。这是玩家设备条件，不是开发者先开 Play Console 的要求；TapTap 下载不代表设备自动满足它。[FAQ](https://docs.unity.com/en-us/grow/ads/references/faq)。
2. 截至核查，没有取得针对本项目 `taptap.io`-only、马来西亚个人收款的明确后台结论或官方答复。能添加应用和显示测试广告均不替代这一步。
3. 2026-04 起直连可能降低广告表现；官方仍表示继续支持并提供广告填充。首轮先评估是否能合规上线与收款，不据此承诺填充率或收入。[当前说明](https://docs.unity.com/en-us/monetization/getting-started/changelog)。
4. Unity 支持个人收款，但付款资料、身份／税务、可用方式和费用须按实际账户核对；不能套用 AdMob 的付款门槛。[FAQ](https://docs.unity.com/en-us/grow/ads/references/faq)、[付款概览](https://docs.unity.com/en-us/monetization/payments/intro-to-monetization-payments)、[付款资料](https://docs.unity.com/en-us/monetization/payments/complete-payout-profile)。

### 2.1 新版后台接入待核实（优先解决）

2026-11-15 跟随用户实际页面核对广告位文档时，发现新版官方文档明确：只能新建 **bidding placements**，无法新建 waterfall placements／ad units；创建说明还写明新版后台当前只支持 mediated users。因此，前面的“Store ID 可选”证据不足以证明本新账号能使用传统独立直连。[广告位总览](https://docs.unity.com/en-us/monetization/dashboard/placements)、[创建说明](https://docs.unity.com/en-us/monetization/dashboard/placements/create-manage-placements)。

当前处理：先查看本账号“广告位 → 创建广告位”的实际可用类型、关联的网络／聚合配置与说明，再核对匹配的 SDK/API。**不直接把新建竞价位 ID 当作旧直连广告单元 ID 使用，也不因为 Game ID 已生成就开始安装传统直连包。** 若必须使用聚合平台，重新核实该平台是否接受 TapTap-only 和马来西亚个人；不默认强行切到 LevelPlay 或 AdMob。

用户最新创建结果已给出 `BP_Rewarded_Android`，但弹窗未说明独立直连或所选聚合平台。进一步核对的官方材料存在冲突：Unity 6000.6 手册写 Advertisement Legacy 包从 2026-01-31 起不再支持直连变现，列出的包版本为 4.19.0；当前 Unity C# SDK 4.21.0 指南又写 2026-04-01 起直连表现可能降低、仍提供填充。这不能通过比较版本数字直接消除，4.21 的文档编号也不能当作已验证可装的 UPM 包版本。[6000.6 包手册](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.ads.html)、[C# 接入指南](https://docs.unity.com/en-us/ads-unity/4.21.0/sdk-integration)。

据此继续保持 C00e 未完成，需核实账号对应接入说明，必要时由 Unity 明确新应用及该竞价位可采用的正式方式。当前不安装 Legacy 包或把测试广告加载成功作为正式资格证据。已创建的应用及广告位作为现有配置保留，不无故重建。

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
- [x] 用户能进入 Unity Monetization，并已添加关联 EdgeRush 的 Android 应用 Edge Rush；截图确认 Game ID `800438692`、Store ID 未设置、普通大众／非儿童定向。未保存完整后台截图或个人支付资料到仓库。
- [x] 用户在新版后台仅选择 Rewarded，成功创建 `BP_Rewarded_Android`；截图有明确创建成功提示。此项仅表示后台配置完成，不表示 SDK 已接通或可获正式广告。
- [ ] 核实本账号新建广告位支持的实际接入方式；新版文档的 bidding-only／mediated-only 与传统独立直连说明需澄清，不能混用 ID 或 SDK API。
- [ ] 明确确认 `taptap.io`-only APK 的正式 rewarded 广告资格、缺 Store ID 的正确配置和审核材料。
- [ ] 明确马来西亚个人付款支持、实际付款方式、最低额、费用、周期、身份／税务要求；只记录状态，不保存银行资料。
- [x] 用户自行完成付款档案前端填报，截图显示四步向导到 Done。Malaysia／MYR／Local Bank Transfer 选项及门槛 USD100、交易费 USD5、另有 FX fee 已有页面证据；不保存银行卡、身份证、税号、出生日期或签名。
- [ ] 付款暂扣解除／税表已收到或接受仍未核实；最新截图背景显示税表尚未收到，不能推定同步中、审核中、已拒绝或已通过，先刷新并查看具体状态。
- [ ] 固定正式包名与应用条目，创建 Game ID／所需 rewarded 广告位，强制开发设备使用测试模式。
- [ ] 改写执行清单 P4／P5／P10／P12 为实际 Unity Ads 流程；以该 SDK 的隐私要求替代 Google UMP 假设。
- [ ] 接入并锁定真实 SDK、替换 Provider 及发布配置校验，完成可靠奖励、隐私与 Android 真机检查。
- [ ] TapTap 公开下载并满足平台要求后，再向正常玩家开放正式广告，核验有效曝光、收入与实际付款。

没有公开条文明确回答的资格问题，先在官方后台核对，再由用户发送下方问题或授权我代发。当前不发送邮件／工单，不上传项目或账户资料。

Cloud 创建请求采用 `coppa:unspecified`；服务端复查实际返回 `coppa:not_compliant`、`kidsStoreCompliance:false`，数据收集／共享推荐值为 unspecified。这里如实保留字段差异，不把 Cloud 字段当作已经完成广告平台的成年／儿童受众、隐私或同意配置；进入 Monetization 后还要核对。

随后用户在 Monetization 受众页选择并创建应用；最新详情截图显示“这是一个面向普通观众的应用，不针对儿童”。应用受众这一项已有截图证据，但隐私同意、广告内容过滤和正式投放资格未因此完成。

### 4.1 当前付款核对入口

用户先进入 **Go to the payout profile**；也可按官方路径 **Payments → Payment Info → Payout → Set up payout profile**。按实际向导核对手机号两步验证、地址、付款方式和适用税务表单；税表不明确时使用后台的 Tax Form Questionnaire，不凭国籍猜测税务身份。实际姓名／地址／银行资料和验证码由用户直接在官方后台处理，本文件只记完成／待审／待办状态。[完成付款资料](https://docs.unity.com/en-us/monetization/payments/complete-payout-profile)。

本次向导已走到 Done；后续以刷新后的 Payment Info／Tax Forms 实际状态为准。若出现缺件、Invalid／Rejected 或其他原因，按该提示处理；若为 Submitted／Pending，则记录等待而不是反复重新提交。暂扣消失和资料接受也不替代实际收入、达到结算条件及银行到账验收。

付款档案使用的二次验证与 Unity 登录的二次验证不同；已登录或有 Organization Owner 身份不等于该验证已完成。当前尚未确认马来西亚账户实际提供哪些收款方式、门槛与费用。[付款说明](https://docs.unity.com/en-us/monetization/payments/intro-to-monetization-payments)。

## 5. 给 Unity Ads 支持的问题草稿（未发送）

官方入口：[Unity Ads Support](https://support-ads.unity.com/s/)。

```text
Subject: Unity Ads eligibility for a TapTap International-only Android game

Hello Unity Ads Support,

I am an individual developer based in Malaysia. I plan to distribute a free
Android Unity game, Edge Rush, only on TapTap International (taptap.io).
The game will not have a Google Play or Apple App Store listing initially.
The app is intended for a general audience and is not directed to children.
Initial testing is with adults in Malaysia, using English.
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
5. The new dashboard has created Android Game ID 800438692, but your placement
   guides state that only bidding placements and mediated users are supported.
   The rewarded network placement ID is BP_Rewarded_Android.
   Can a new publisher use Unity Ads without a mediation provider? If yes, which
   dashboard flow, SDK/package version, placement type and load API must be used?
6. The Unity 6000.6 package manual says direct monetization through Advertisement
   Legacy is no longer supported from January 31, 2026, while the Unity C# 4.21
   SDK guide says direct integrations continue to receive fill. Which applies
   to this newly created app and placement?

Please distinguish test-ad access from approval for live monetization and payouts.
Thank you.
```

草稿未包含密码、证件、银行账户或真实广告凭据。取得答复后在主清单 C00 记录可核对结论；不将泛泛的“支持 Android”答复视为所有问题已解决。
