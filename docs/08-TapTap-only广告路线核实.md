# Edge Rush · 仅 TapTap 国际版广告路线核实

> 核查日期：2026-11-15（Asia/Singapore）
>
> 状态（2026-11-22 更新）：用户提供 Unity Grow Support 工单 **00945668** 的答复截图：当前新竞价位不能 standalone，需 LevelPlay 等聚合平台发送 ad markup；Unity Ads／LevelPlay 正式广告及收入需要 Google Play 或 App Store 条目，**TapTap-only 不受支持**。原先优先验证直连的候选结论已失效，不能继续等回信或安装该路线 SDK。Cloud／Game ID／竞价位与 Verified 付款档案保留；用户尚未授权改变仅 TapTap 发行范围。

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
- 付款状态：最新 Payment Info 截图明确显示 **Payout profile: Verified**，先前税表未收到／付款暂扣红条已不再显示。付款档案验证完成；不把它扩大为广告应用批准、达到付款条件或款项到账。

## 2. 最新结论：Unity 已否定本项目当前 TapTap-only 路线

2026-11-22 用户提供的邮件截图显示，回复来自 **Unity Grow Support**（不是 TapTap），工单 **00945668**，回复者 **Smiltė**；邮件界面显示 2026-11-17 22:19，时区未独立核实。本轮依据用户截图记录，没有独立认证邮箱／完整邮件头，详细答复与历史询问见 [10](10-Unity广告接入支持询问.md)。

- `BP_Rewarded_Android` 是新竞价位，需要聚合平台在请求中发送 ad markup，普通 standalone load/show 不适用。
- 新项目支持路径为 LevelPlay Ads Mediation（`com.unity.services.levelplay`）加 Unity Ads bidder；Legacy／4.21.0 直连说明是已有 placements 的说明。
- **正式投放与收入需要 Google Play 或 Apple App Store 条目；TapTap 不支持，temporary app 只能先设置。** Verified payout profile 只代表收款档案。
- 现有 Game ID／竞价位可按手动 bidder 配置复用，测试广告仅证明技术集成；LevelPlay 账号审核仍基于已发布应用，没有因此批准当前游戏。

当前先保留已有配置，不重建、不装 Legacy／LevelPlay SDK、不重复发原询问。用户仍选择 TapTap-only；下一步是核实其他平台是否明确接受 **TapTap International-only、马来西亚个人和实际收款**。若无适合路线，再请用户决定是否增加受支持商店，不能静默改成 Google Play。签名备份／恢复、设备和商店身份准备可以独立推进。

### 2.1 历史候选依据（2026-11-15，已被上述答复覆盖）

早期研究发现 Store IDs 可选、添加应用有“尚无商店 URL”流程，因此曾把 Unity Ads 直连列为待验证候选。这些事实**不足以证明新竞价位接法或正式商店资格**；工单00945668已明确当前提案不适用。以下链接和旧观察仅保留溯源，不作为继续该路线的指令。[流程](https://docs.unity.com/en-us/grow/ads/process-overview)、[Store IDs](https://docs.unity.com/en-us/grow/dashboard/get-started/project/settings#store-ids)、[添加应用](https://docs.unity.com/en-us/monetization/dashboard/add-an-app)。

当时记录的未决条件如下；其中接法与商店条件现已由答复澄清：

1. 官方 FAQ 对站外 Android 分发提示：广告收益还依赖玩家设备能够访问 Google Play。这是玩家设备条件，不是开发者先开 Play Console 的要求；TapTap 下载不代表设备自动满足它。[FAQ](https://docs.unity.com/en-us/grow/ads/references/faq)。
2. 截至2026-11-15尚无针对本提案的答复；2026-11-22已收到的截图现明确TapTap-only不满足该路线正式商店条件。
3. 当时指南称2026-04起直连可能降低表现、仍提供填充；支持现解释这属于已有placements，不能沿用到当前新竞价位。[历史研究来源](https://docs.unity.com/en-us/monetization/getting-started/changelog)。
4. Unity 支持个人收款，但付款资料、身份／税务、可用方式和费用须按实际账户核对；不能套用 AdMob 的付款门槛。[FAQ](https://docs.unity.com/en-us/grow/ads/references/faq)、[付款概览](https://docs.unity.com/en-us/monetization/payments/intro-to-monetization-payments)、[付款资料](https://docs.unity.com/en-us/monetization/payments/complete-payout-profile)。

### 2.2 历史新版后台接入疑点（现已答复）

2026-11-15 跟随用户实际页面核对广告位文档时，发现新版官方文档明确：只能新建 **bidding placements**，无法新建 waterfall placements／ad units；创建说明还写明新版后台当前只支持 mediated users。因此，前面的“Store ID 可选”证据不足以证明本新账号能使用传统独立直连。[广告位总览](https://docs.unity.com/en-us/monetization/dashboard/placements)、[创建说明](https://docs.unity.com/en-us/monetization/dashboard/placements/create-manage-placements)。

当时处理是暂缓安装、向官方核对。现已确认**不直接把新竞价位当旧直连广告单元**；支持同时否定当前TapTap-only的LevelPlay正式资格，不能仅把SDK改为聚合就声称已解决发行限制。

用户最新创建结果已给出 `BP_Rewarded_Android`，但弹窗未说明独立直连或所选聚合平台。进一步核对的官方材料存在冲突：Unity 6000.6 手册写 Advertisement Legacy 包从 2026-01-31 起不再支持直连变现，列出的包版本为 4.19.0；当前 Unity C# SDK 4.21.0 指南又写 2026-04-01 起直连表现可能降低、仍提供填充。这不能通过比较版本数字直接消除，4.21 的文档编号也不能当作已验证可装的 UPM 包版本。[6000.6 包手册](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.ads.html)、[C# 接入指南](https://docs.unity.com/en-us/ads-unity/4.21.0/sdk-integration)。

该接法疑点现由工单00945668澄清；这表示核实得到了明确答复，**不是原路线已通过上线门槛**。保留已有配置和研究记录，Unity候选在当前TapTap-only范围内停止接入；尚未安装真实广告SDK。

## 3. 候选比较与取舍

| 候选 | 可证明的事实 | 当前处理 |
| --- | --- | --- |
| AdMob | 受支持商店列表不含 TapTap；只在不受支持商店发布的应用无法完成 readiness 审核，广告受限 | 保留账号；不把它当作当前正式变现已解决。[应用设置](https://support.google.com/admob/answer/9989980?hl=en) |
| Unity Ads 直连 | 工单00945668明确新竞价位不能standalone，旧说明针对已有placements | 当前配置不走直连；不安装Legacy包 |
| LevelPlay／ironSource | 工单00945668明确正式广告／收入需Play或App Store条目；TapTap不支持、Temp不够production | 当前TapTap-only路线不成立；未授权新增商店，暂不接入。[账户](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/create-account)、[应用](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/add-app) |
| AppLovin MAX | 官方说明替代商店并非正式支持范围，可手工添加但风险自行承担 | 不优先用于此次需要明确上架／收款路径的首轮验证。[官方说明](https://support.applovin.com/en/max/max-dashboard/ad-units/create-an-ad-unit) |
| Mintegral | 官方 API 支持未上商店、`other` 商店和 rewarded video | 可作第二候选；仍需确认 `.io` 流量、马来西亚个人与付款。[API](https://cdn-adn-https.rayjump.com/cdn-adn/open_api/MintegralAPI.html) |
| TopOn | 允许手工添加和测试；API 列 TapTap，但示例为 `.cn`；第三方源各自结算 | 聚合本身不保证广告需求或付款。ADX 需另谈协议，暂不先增加聚合层。[账号／结算](https://help.toponad.net/docs/MfO9IZ)、[API](https://help.toponad.net/docs/App-API-v3) |

Mintegral 和 TopOn ADX 的通用条款都存在低于 US$1,000 默认累积、低额申请由开发者承担银行费用等约定。它们不是简单的“完全不能低额提款”，但小流量项目可能较难达到经济上合适的首次收款；最终要看实际协议与费用。[Mintegral §5.2](https://dev.mintegral.com/document/term-en.pdf)、[TopOn §3.2.5](https://www.toponad.net/en/terms)。

## 4. 依次执行的核验门槛

- [x] 用户确认 TapTap-only、个人、马来西亚、公开开发者资料及首轮试玩范围。
- [x] 初查主要候选的商店限制，曾选择Unity Ads直连核实；现已收到否定当前提案的明确答复，此历史勾选不是路线批准。
- [x] Unity CLI `auth status` 确认已有 Thingamajig 登录会话，邮箱与用户提供的公开邮箱一致；无需先重复注册。Unity Cloud 组织与 Monetization 权限单独核对。
- [x] Unity CLI `cloud org` 确认已有 Cloud 组织且账号角色为 Owner；没有新建组织，也没有修改权限。此角色不替代广告应用审核或收款验证。
- [x] 在该组织只读搜索名称含 Edge 的 Cloud 项目，返回空；没有创建新项目。这个结果不等于已查遍旧 Monetization 后台的所有项目。
- [x] 用户表示 Cloud 里尚无项目后，再列出该组织全部项目为空；通过 Unity 插件创建并关联 **EdgeRush**，远端复查返回该项目，ID `19ff5aec-d901-42b0-aa48-545cc9fcf640`。这不是广告 Game ID／广告位 ID。
- [x] 编辑器用 `--if-saved` 正常关闭（unsaved 为空），重新打开后为 ready；插件读取 `Application.cloudProjectId` 与新项目 ID 一致，厂商和拟定包名保留。
- [x] 用户能进入 Unity Monetization，并已添加关联 EdgeRush 的 Android 应用 Edge Rush；截图确认 Game ID `800438692`、Store ID 未设置、普通大众／非儿童定向。未保存完整后台截图或个人支付资料到仓库。
- [x] 用户在新版后台仅选择 Rewarded，成功创建 `BP_Rewarded_Android`；截图有明确创建成功提示。此项仅表示后台配置完成，不表示 SDK 已接通或可获正式广告。
- [x] 核实本账号新广告位接法：工单00945668明确需mediator提供ad markup，不能standalone；建议LevelPlay Ads Mediation加UnityAds bidder，版本未指定。
- [x] 取得本提案商店资格的明确答复：**TapTap-only不受支持**，正式广告／收入需Play或App Store条目。这是核实完成／结论为不符合，不是广告资格通过。
- [x] 本账户的马来西亚个人收款档案已通过验证，实际提供方式与费用已核对；付款周期与具体结算条件在首次真实收入后按当时后台复核，不保存银行资料。
- [x] 用户自行完成付款档案前端填报，截图显示四步向导到 Done。Malaysia／MYR／Local Bank Transfer 选项及门槛 USD100、交易费 USD5、另有 FX fee 已有页面证据；不保存银行卡、身份证、税号、出生日期或签名。
- [x] 最新截图显示 Payout profile: Verified，原税表未收到／付款暂扣提示不再显示。仅确认后台当前状态；不另推断尚未展示的内部税表审核细节。
- [ ] 固定正式包名与应用条目；Game ID／竞价位已创建并保留，测试模式尚未验证。只有路线成立后才继续该网络的真实接入。
- [ ] 核实其他广告平台是否接受TapTap国际版-only及马来西亚个人收款；若需要改变发行范围，先由用户决定。
- [ ] 对最终获准路线改写执行清单和隐私／付款／应用审核步骤；不再默认写成Unity Ads接入。
- [ ] 路线明确后接入并锁定真实SDK、替换Provider及发布校验，完成可靠奖励、隐私与Android真机检查。
- [ ] TapTap 公开下载并满足平台要求后，再向正常玩家开放正式广告，核验有效曝光、收入与实际付款。

用户已提供 [10 的具体支持询问](10-Unity广告接入支持询问.md)的回复截图，工单00945668；当前接法与商店范围按这份答复执行，不再把“等待Unity回信”列为当前阻塞。2026-11-22用户又单独授权[12的Mintegral资格询问](12-Mintegral资格核实与询问草稿.md)，已从支持邮箱发给官方developer@mintegral.com，Gmail显示Message sent；**等待的是Mintegral适用答复，尚无替代正式资格**。两个原询问都不重复发送，不把发送或测试成功当批准，不上传代码、日志、身份证明或付款资料。

Cloud 创建请求采用 `coppa:unspecified`；服务端复查实际返回 `coppa:not_compliant`、`kidsStoreCompliance:false`，数据收集／共享推荐值为 unspecified。这里如实保留字段差异，不把 Cloud 字段当作已经完成广告平台的成年／儿童受众、隐私或同意配置；进入 Monetization 后还要核对。

随后用户在 Monetization 受众页选择并创建应用；最新详情截图显示“这是一个面向普通观众的应用，不针对儿童”。应用受众这一项已有截图证据，但隐私同意、广告内容过滤和正式投放资格未因此完成。

### 4.1 当前付款核对入口

用户先进入 **Go to the payout profile**；也可按官方路径 **Payments → Payment Info → Payout → Set up payout profile**。按实际向导核对手机号两步验证、地址、付款方式和适用税务表单；税表不明确时使用后台的 Tax Form Questionnaire，不凭国籍猜测税务身份。实际姓名／地址／银行资料和验证码由用户直接在官方后台处理，本文件只记完成／待审／待办状态。[完成付款资料](https://docs.unity.com/en-us/monetization/payments/complete-payout-profile)。

最新 Payment Info 已显示 Verified，无需再次填写向导。未来若重新出现缺件、Invalid／Rejected 或其他原因，按该提示处理；若为 Submitted／Pending，则记录等待而不是反复重新提交。Verified 也不替代实际收入、达到结算条件及银行到账验收。

付款档案使用的二次验证与 Unity 登录的二次验证不同；已登录或有 Organization Owner 身份不等于该验证已完成。本账户实际页面已提供 Malaysia／MYR／Local Bank Transfer，最低付款门槛 USD100、交易费 USD5、另有 FX fee；后续付款条件以当时后台为准。[付款说明](https://docs.unity.com/en-us/monetization/payments/intro-to-monetization-payments)。

## 5. 支持询问状态与历史草稿

**当前询问内容与回复记录以 [10-Unity广告接入支持询问](10-Unity广告接入支持询问.md) 为准。** 用户2026-11-22提供Unity Grow Support工单00945668的回复截图；当前新竞价位需聚合，而TapTap-only正式投放不受支持。不重复发送原询问，不把这份否定性答复写成资格批准。

以下六问为早先研究阶段的历史版本，保留供追溯，**不再直接复制发送**；付款档案目前已 Verified，当前稿聚焦新版 SDK／竞价位及 TapTap-only 正式资格。

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

草稿未包含密码、证件、银行账户或真实广告凭据。工单00945668的具体答复已记录于10，主清单C00也应同步为“接法已澄清、当前TapTap-only Unity路线不符合”，不能标为投放批准。后续其他平台的泛泛“支持Android”同样不能视为本项目全部资格已解决。
