# Unity Ads integration support inquiry

Status: **REPLY RECEIVED — TICKET 00945668; TAPTAP-ONLY PRODUCTION ROUTE NOT SUPPORTED**

Prepared: 2026-11-15

Reply recorded: **2026-11-22**, from the publisher's supplied email screenshot. The email interface shows **Tue, Nov 17, 10:19 PM** (2026); the displayed timezone and message headers have not been independently verified.

Official recipient: [Unity Ads Support](https://support-ads.unity.com/s/)

Purpose: Confirm the supported production monetization path and eligibility for a new Android app distributed exclusively through TapTap International: standalone Unity Ads, Unity Ads bidding, or LevelPlay.

The publisher explicitly authorized submission of this draft on 2026-11-16, and subsequently confirmed successful submission. On 2026-11-22, they supplied the response for **ticket 00945668** from **Unity Grow Support**, displayed sender `growth-support@unity3d.com`, responder **Smiltė, Specialist, Ads SMB Support**. This is Unity's response, not a TapTap support reply. Initially recorded from the supplied screenshot; later the same subject, sender, displayed date and body were also observed in the already signed-in `wkeongg@gmail.com` Gmail interface while sending a separately authorized Mintegral inquiry. Full message headers/authentication results and the linked support ticket were not opened or independently authenticated. Do not send a duplicate request. No bank, identity, tax, credential, or signing-key data may be attached.

The official [Contact Us form](https://support-ads.unity.com/s/ContactUs) is filled in Chrome with the public developer name/email, the message below and its official source URLs. Product: Unity Ads Monetization; category: Technical issue; sub-category: SDK Integration assistance; Game ID: 800438692; mediation: Other. Both SDK-version fields truthfully state that integration has not started. No file is attached.

The first automated attempt displayed **Please verify you're not a robot**. Computer control later stopped because it could not reliably verify the browser URL. After resuming, the publisher completed the submission themselves and explicitly reported success. That earlier event was submission evidence only; the subsequently supplied reply below now resolves the direct-integration and TapTap-only questions for this proposed setup.

## 已收到的答复与当前决定（优先于后面的历史草稿）

**结论：现有 `BP_Rewarded_Android` 不能以 standalone load/show 接入；本项目若仍只在 TapTap 国际版发行，也不满足该回复所述 Unity Ads／LevelPlay 正式广告与收入条件。不能继续把这条路线当作“待回信即可接入”。**

按截图概括支持答复：

1. 新后台只创建竞价位，每次广告请求需要由聚合平台发送 ad markup；普通直连 load/show 不发送该内容。先前引用的 Advertisement Legacy 和 4.21.0 直连说明适用于已有 placements，不能据此证明当前新竞价位支持直连。
2. 新项目的支持路径是 **LevelPlay 的 Ads Mediation 包 `com.unity.services.levelplay`，将 Unity Ads 配为 bidder**。邮件没有指定可直接安装的确切包版本。
3. Unity Ads／LevelPlay 支持通过 Google Play 或 Apple App Store 分发的应用，正式广告和收入需要其中一个商店条目；**TapTap 不属于支持商店**。未上线应用可以先用临时名称设置，但临时应用不能替代 production 条件。
4. 已 Verified 的 payout profile 只处理收款，不替代商店条件或投放资格。
5. Game ID `800438692` 和广告位 `BP_Rewarded_Android` 可以通过邮件所指的 Unity Ads 手动 bidder 配置复用；配置复用不等于获准正式投放。
6. 测试广告仅证明集成工作；LevelPlay 账号还有基于已发布应用的审核。支持建议在应用上线 Google Play 后进行相关审核。

邮件提供的官方参考（本轮仅记录截图里的链接，没有打开邮件链接或对外回复）：

- [LevelPlay Unity package integration](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/package-integration)
- [Unity Ads bidder configuration](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/networks/guides/unity-ads)
- [LevelPlay add an app](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/add-app)
- [LevelPlay create an account](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/create-account)

**执行边界：** 用户目前仍选择 TapTap-only，没有授权改为 Google Play／App Store 发行。保留既有 Cloud 项目、Game ID、竞价位和 Verified 收款档案；不重建、不安装 Legacy／LevelPlay SDK、不把条目添加或测试成功记成正式批准。接下来核实其他明确接受 TapTap International-only 分发、马来西亚个人和收款方式的广告平台；若没有适合路线，再由用户决定是否改变分发范围。签名备份／恢复和设备准备可以独立继续。

本答复没有提供马来西亚个人的额外限制／豁免、其他渠道的资格、准确 SDK 版本、所有剩余审核材料或新增收款 onboarding 的完整清单。不得将这些未答事项补写为已批准；当前商店条件已足以否定所提的 TapTap-only Unity 路线。

## Historical inquiry — retained for traceability; do not resend

## Subject

TapTap-only Android app: Unity Ads / LevelPlay production eligibility

## Message

Hello Unity Ads Support,

I am an individual publisher based in Malaysia, preparing to release Edge Rush as an Android APK exclusively through TapTap International. The app has no Google Play listing, and I do not plan to publish this release on Google Play.

My current Unity Monetization configuration is:

- App: Edge Rush, Android
- Game ID: `800438692`
- New bidding placement ID: `BP_Rewarded_Android` (rewarded)
- Payout profile: Verified
- Google Play Store ID: Not set
- Audience designation: General audience, not directed to children
- Unity Editor: `6000.6.7f1`

I understand that payout verification alone does not establish app, ad-serving, or LevelPlay production approval. Before choosing an SDK integration, could you please confirm the following three points?

1. **Standalone Unity Ads:** Can this new bidding placement serve production rewarded ads through standalone Unity Ads, without an external mediator? If so, which Unity package ID/version and integration guide are supported for this configuration? In particular, can the bidding placement be used with the ordinary direct load/show flow, or does it require a mediation auction and bidding ad markup? If direct integration is unavailable to a new account, please confirm that explicitly.

2. **TapTap-only production eligibility:** If LevelPlay is required, can a new LevelPlay account and an Android app distributed only through TapTap International be approved to serve paid Unity Ads bidding traffic without a Google Play listing? Is a live TapTap listing URL sufficient for review? Please identify any additional publication, store, traffic, country, or individual-publisher eligibility requirements, including whether an unpublished app can be approved before its TapTap release.

3. **Reuse and remaining approvals:** Can I reuse Game ID `800438692` and bidding placement `BP_Rewarded_Android` through LevelPlay's manual Unity Ads bidder setup, rather than creating another Unity Monetization app? Which remaining account/app/network approvals, review materials, store metadata, and test-to-live steps are required before production traffic is permitted? Please distinguish a successful test-ad integration from approval to serve paid live ads, and confirm whether my existing verified payout profile applies to this setup or whether additional payment onboarding is required.

The official documentation below appears inconsistent about standalone support and new-dashboard placement usage. Please clarify which guidance applies to this new account and placement, and provide the current supported documentation or required account configuration.

Thank you.

## Historical documentation observations that prompted the inquiry

Sources reviewed on 2026-11-15. These were the observations that prompted the inquiry, not confirmation of production eligibility. The support response above takes precedence for this new app and placement; the direct-integration documentation does not establish TapTap-only approval.

- [Unity 6.6 manual: Advertisement Legacy](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.ads.html): Says direct monetization using this package is unsupported from January 31, 2026, and recommends LevelPlay. The same page lists package version 4.19.0 for Unity 6000.6.
- [Unity Ads SDK 4.21.0 integration guide](https://docs.unity.com/en-us/ads-unity/4.21.0/sdk-integration): Says direct integrations may see reduced performance from April 1, 2026, while Unity Ads continues supporting and filling them. It also says mediated integrations should follow their adapter guide to avoid duplicate initialization.
- [New-dashboard placement creation](https://docs.unity.com/en-us/monetization/dashboard/placements/create-manage-placements): States the New Unity Dashboard currently supports mediated users only. [Placement overview](https://docs.unity.com/en-us/monetization/dashboard/placements) says only bidding placements can now be created.
- [Dashboard changes for publishers](https://docs.unity.com/en-us/monetization/dashboard/dashboard-changes): Includes instructions to use placement IDs in app code when not using mediation, which does not make the availability of that path for newly created bidding placements clear.
- [LevelPlay: Add an app](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/add-app): Allows a temporary app entry when an app is not live in the App Store or Google Play. [LevelPlay: Create an account](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/create-account) separately describes account approval and requests for information about published apps. These pages do not explicitly confirm TapTap-only production eligibility.
- [Unity Ads bidder configuration in LevelPlay](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/networks/guides/unity-ads): Documents manual bidder setup with a Game ID and Placement ID. Confirmation is requested that the existing app and placement above can use that route.

## Reply record — ticket 00945668

| Item to record | Confirmed answer / reference |
| --- | --- |
| Ticket number, submission date, responding team, and reply date | 00945668; publisher confirmed submission 2026-11-16; Unity Grow Support, Smiltė; email UI shows 2026-11-17 22:19, timezone not independently verified; screenshot supplied 2026-11-22 |
| Supported route for this new account: standalone, LevelPlay bidder, or other | LevelPlay Ads Mediation with Unity Ads as bidder; current TapTap-only distribution does not satisfy the stated production store condition |
| Exact supported Unity package ID/version and integration guide | `com.unity.services.levelplay`; package integration and Unity Ads bidder guides linked above; exact version not specified |
| Whether the existing bidding placement is valid for direct load/show | No; mediation must supply ad markup with requests |
| TapTap-only eligibility; accepted listing/review evidence; unpublished-app policy | TapTap is not a supported store; a Google Play or Apple App Store listing is required for live ads/revenue. Temporary-name setup is not enough for production |
| Store, traffic, Malaysia, and individual-publisher requirements | Store condition answered as above; no separate Malaysia/individual exception or complete traffic requirements supplied |
| Reuse of Game ID and placement; any dashboard configuration changes | Game ID 800438692 and BP_Rewarded_Android can be reused through the manual Unity Ads bidder setup; this is configuration reuse, not production approval |
| Outstanding account/app/network approvals and responsible review team | LevelPlay account review is based on published apps; support suggests review once live on Google Play. No approval has been granted to this app |
| Payment-profile reuse or additional payment onboarding | Verified payout profile covers payouts only; complete additional onboarding/reuse conditions were not specified |
| Test-mode procedure and explicit conditions for enabling live traffic | Test ad success proves integration only; no app approval or complete test-to-live procedure supplied |
| Official explanation of the conflicting documentation and applicable policy date | Legacy/4.21.0 direct documentation applies to existing placements; new bidding placement needs mediation. This is the support explanation supplied for this setup, not a universal approval of older direct integrations |

Record the support team's exact scope and conditions. An integration test, payout verification, or generic statement that Android is supported must not be recorded as approval of TapTap-only production monetization.
