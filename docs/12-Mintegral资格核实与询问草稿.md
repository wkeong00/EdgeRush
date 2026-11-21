# Mintegral：TapTap 国际版资格核实与可审核询问草稿

> 整理／官方网页查核：2026-11-22（Asia/Singapore）。
>
> 状态：**用户已单独授权，2026-11-22（Asia/Singapore）已通过 Gmail 发送一次，界面实际显示 Message sent／View message。等待资格答复，不要重复发送。**
>
> 本文件不表示已经注册 Mintegral、通过应用审批、选定 SDK、获得正式广告填充或取得收入。不要据此安装 SDK 或切换发行路线。

## 1. 当前决策与需要解决的问题

- 用户当前仍选择仅发行 TapTap International（`taptap.io`），暂不新增 Google Play 或 Apple App Store。
- Mintegral 是**已发出资格询问的第一候选**，不是已成立的正式盈利路线。
- 优先问清能否使用 Mintegral 的 direct SDK 或自建聚合模式，不依赖 AdMob／LevelPlay。
- 若只有竞价方式可用，须由官方明确可用的自建聚合／竞价流程及必要权限；不能将“支持激励视频”推断为任意直连接法都获支持。
- 只有正式资格、具体 SDK／模式、审批与结算条件明确后，才决定技术接入。
- 注册成功、API 可以建立应用、测试广告加载成功，都不能代替正式付费流量批准。

## 2. 已查到的官方证据及边界

| 官方资料 | 可以确认的事实 | 不能由此推断的事项 |
| --- | --- | --- |
| [Mintegral Developer Interface Documentation](https://cdn-adn-https.rayjump.com/cdn-adn/open_api/MintegralAPI.html) | 创建应用 API 有 `is_live_in_store=0`；商店枚举含 `other`；广告格式含 `rewarded_video`，标为 Native SDK | TapTap.io-only 正式流量、当前新账号审批、马来西亚个人付款或具体 Unity direct／自建聚合模式已获批准 |
| [Mintegral Publisher Terms](https://dev.mintegral.com/document/term-en.pdf) | 2026-04-29 版本适用于个人与法人；相应非中国大陆／美加主体与 MINTEGRAL PTE. LTD 签约；美元结算有申请／确认及费用条款 | 本项目或该个人一定可完成开户、接受马来西亚流量、无最低流量要求或一定能低成本收款 |
| [Mintegral Contact](https://www.mintegral.com/en/contact) | Monetization 官方联系地址为 `developer@mintegral.com`；SDK 支持为 `sdk-support@mintegral.com` | 联系地址存在不代表已经发出询问或已取得答复 |

API 里的 `other` 是候选依据，不是 TapTap International 的点名批准。资料没有明确把 `taptap.io` 与中国大陆 `taptap.cn` 视为同一可接受分发来源。

### 2.1 小流量结算的已知约束

依据上述 Publisher Terms §5.2：美元付款通常在付款申请且平台确认后45天内；六月／十二月到期有另行延期规定。

低于 US$1,000 的申请金额通常累积至之后月份；开发者要求低额付款时可付款，但相关全部银行费用由开发者承担；不足以覆盖银行费用的金额仍累积。

**不能把它写成“绝对最低1000美元、低于1000不能提”。** 实际账户、个人所需文件、币种、费用、低额申请方式和适用协议仍应由官方确认；本项目当前也没有真实收入。

## 3. 本次询问采用的项目事实

| 字段 | 可提供内容 |
| --- | --- |
| 游戏 | `Edge Rush` |
| 开发者公开名称 | `Thingamajig` |
| 厂商显示标签 | `Gizmobyte`；个人开发者使用的公开标签，不表述为已注册公司 |
| 主体／所在国家 | 马来西亚个人 |
| 平台／引擎 | Android APK，Unity `6000.6.7f1` |
| 拟定包名 | `com.gizmobyte.edgerush`；尚待首次商店上传核对 |
| 分发范围 | 计划仅 TapTap International（`taptap.io`），不含 Google Play／App Store 条目 |
| 公开状态 | 尚未发布；目前没有可提供的正式游戏商店条目 URL |
| 广告状态 | 当前为内部 Mock 测试，没有真实广告 SDK 接入或真实广告收入 |
| 希望的广告 | 用户自愿选择的激励视频，优先复活与结算奖励 |
| 测试对象 | 首轮英文、马来西亚成年测试者，小范围 |
| 应用受众声明 | General audience，not child-directed；不能等同于已完成18+内容分级 |
| 网站 | https://wkeong00.github.io/EdgeRush/ |
| 隐私政策 | https://wkeong00.github.io/EdgeRush/privacy.html |
| 支持页面 | https://wkeong00.github.io/EdgeRush/support.html |
| 公开联系邮箱 | `wkeongg@gmail.com` |

网站政策当前描述内部 Mock 测试版。真实 SDK 加入前要按实际数据实践修改；不能把现有政策描述为已经披露 Mintegral 的数据采集。

未提供活跃用户量、下载量或收入预测，不虚构最低流量已经满足。

## 4. 收件人、发送状态与附件

- 实际收件人：`developer@mintegral.com`。
- 地址来源：[Mintegral 官方联系页](https://www.mintegral.com/en/contact)，Monetization 栏。
- 当前不抄送其他平台或付款团队；如官方要求，再按实际问题处理。
- 实际发送日期／渠道：2026-11-22（Asia/Singapore），Chrome 中的 Gmail；发件人为 `Thingamajig <wkeongg@gmail.com>`。
- 用户已对这份 Mintegral 收件人及英文草稿**单独明确授权发送**；不依赖之前的 Unity 询问授权。
- 本次无附件，主题与第5节正文按下方版本发送，没有附 APK 或后台原件。
- 可见回执：Gmail 先显示 Sending，随后实际显示 **Message sent** 与 **View message**；发送成功界面已被观测。
- 尚未打开已发送邮件的具体 thread；未取得 message ID、Mintegral 工单号、送达确认或对方回复。界面发送成功不等于收件方已送达、已审阅或批准。
- **不要重复发送。** 后续在原邮件／原回复中继续，先等待适用资格答复。

## 5. 已发送的英文询问正文

以下主题及英文正文已按用户授权于2026-11-22发送；保留作后续回复核对，不是待再次发送的草稿。

### Subject

```text
TapTap International-only Android app: production rewarded ads and Malaysia individual eligibility
```

### Message

```text
Hello Mintegral Monetization Team,

I am an individual developer based in Malaysia, preparing Edge Rush, a portrait Android game built with Unity 6000.6.7f1. My public developer name is Thingamajig and my publisher label is Gizmobyte; I am applying as an individual, not as a registered company.

The proposed Android package name is com.gizmobyte.edgerush. I plan to distribute the APK only through TapTap International (taptap.io), not through Google Play or the Apple App Store. The game is not publicly released yet, so I do not currently have a live store listing URL. It has no live ad integration or ad revenue; the internal build only simulates rewarded ads.

The app is intended for a general audience and is not child-directed. Initial testing is planned with English-speaking adult testers in Malaysia. I would like to offer optional rewarded videos for reviving a run and increasing settlement rewards.

Before creating an account or integrating an SDK, please confirm:

1. Can this exact TapTap International-only distribution model be approved for paid rewarded-video traffic without a Google Play or Apple App Store listing? Please distinguish taptap.io from taptap.cn. Is a live taptap.io listing sufficient for review, and can an unpublished app be reviewed before release? What evidence is required?

2. Which current Unity SDK/package and version support this setup directly or with in-house mediation, without AdMob or LevelPlay? If bidding or an approved mediation partner is required instead of ordinary direct load/show calls, please identify the supported integration and any permissions or commercial requirements.

3. Which account, app and traffic approvals must be completed before paid live ads can be enabled? How should test ads and test devices be configured, and what specific conditions permit switching from tests to live traffic?

4. Are there restrictions for a Malaysia-based individual publisher, Malaysian users, player devices or distribution sources? Are there minimum traffic, daily-active-user, impression or published-app requirements? Our game has no established live traffic or revenue yet.

5. What publisher website, app-ads.txt, ownership verification, privacy disclosures, consent or privacy-choice flows are required for this setup? Our current policy describes an internal mock-ad beta and will be updated before any live SDK is added.

6. Can an individual based in Malaysia receive direct payouts? Please specify the required personal or payment documents, supported payment methods and currencies, settlement schedule, thresholds and fees. Your publisher terms describe accumulation below US$1,000 and requested lower-value payments with fees borne by the publisher; please confirm the rules and request procedure that would actually apply to this account.

Website: https://wkeong00.github.io/EdgeRush/
Current internal-beta privacy policy: https://wkeong00.github.io/EdgeRush/privacy.html
Support: https://wkeong00.github.io/EdgeRush/support.html

Please distinguish access to configuration APIs or successful test ads from approval to serve paid live traffic and receive payouts. If this setup is not eligible, please state that explicitly before we begin integration.

Thank you,
Thingamajig
Gizmobyte (publisher label; individual developer)
Public contact: wkeongg@gmail.com
```

## 6. 发送回执与答复判断记录

| 事项 | 当前状态／需取得的结果 |
| --- | --- |
| 用户单独发送授权 | **已取得**；用户明确批准发送本文件的收件人和草稿 |
| 实际发送日期／渠道／回执 | **2026-11-22，Chrome Gmail，Thingamajig <wkeongg@gmail.com> → developer@mintegral.com**；实际可见 Message sent／View message，无附件 |
| message ID／thread／工单号 | 尚未打开已发送具体thread，未取得message ID或Mintegral工单号；不要猜测或重复发送以获取编号 |
| 送达／回复状态 | 无已取得的对方送达确认或回复；Gmail发送成功不等于商业资格批准 |
| TapTap.io-only 正式资格 | 待明确答复；通用“支持 Android／其他商店”不够 |
| 未发布应用审核与正式商店证据 | 待明确接受的资料和时点 |
| SDK／版本／direct或in-house模式 | 待可执行的准确官方指引，不凭 API 字段自行推断 |
| 马来西亚个人与设备／流量条件 | 待明确适用条件和最低量要求 |
| 测试转正式与剩余审批 | 待列出账号、应用、流量审核及正式开通证据 |
| 政策／同意／app-ads.txt | 待最终接法对应要求和实际平台 ID |
| 个人收款、费用和低额提款 | 待实际协议与账户条件；不能直接沿用别的平台 |

如对方只回复测试接入指南，应继续在同一询问中澄清商业资格；不把泛泛答复或测试通过写成已获正式批准。否定答复应记录适用范围，并停止该候选的代码接入准备。

## 7. 第二候选：TopOn 的适用边界

- [TopOn App API](https://help.toponad.net/docs/App-API-v3)列出 TapTap，但示例是 `https://www.taptap.cn/...`，没有据此确认国际版 `taptap.io`。
- [账号／结算说明](https://help.toponad.net/docs/MfO9IZ)明确第三方广告源使用开发者自己的账号、分别结算；免费聚合不绕过各网络资格限制。
- [TopOn ADX](https://help.toponad.net/docs/TopOn-ADX)为自有需求渠道，需联系商务签合作协议；不是添加任意第三方源就由TopOn付款。
- [现行条款](https://www.toponad.net/en/terms)允许自然人，但仍有身份／收款审核和发票或等效账单要求；低于 US$1,000 存在累积／低额全银行费规则。
- 因此只能作为第二个待问候选，未获 TapTap.io-only／马来西亚个人的正式资格，不先注册／安装 SDK。

## 8. 若用户以后愿新增 Google Play：已查到的新依赖

这只是备选成本说明，**用户当前未选择，不能自行执行注册、付费或切路线。**

| 新事项 | 官方当前要求／来源 |
| --- | --- |
| 账号注册 | 一次性 US$25，需身份验证；[注册说明](https://support.google.com/googleplay/android-developer/answer/6112435?hl=en)、[资料要求](https://support.google.com/googleplay/android-developer/answer/13628312?hl=en) |
| 新个人账号闭测 | 2023-11-13之后创建的个人账号需至少12名测试者连续加入14天，然后申请 production access；仍可能被要求继续测试；[闭测要求](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en) |
| 实体设备 | 新个人账号所有者用Play Console手机应用，在非root实体Android 10+手机完成设备验证；模拟器不替代；[设备要求](https://support.google.com/googleplay/android-developer/answer/14316361?hl=en) |
| 商店交付 | 另做Play包／条目／数据和内容声明、审核与正式发布；闭测满14天不是保证即时上线 |

本项目最后用户确认没有可用Android真机，所以实体设备也是实际新增依赖。新增Play条目可以形成另一条路线，但不能由此预先承诺任何广告平台会自动批准投放、收入或付款。

## 9. 本轮结束后的下一步

1. 保留现有 TapTap-only 选择，先完成不依赖广告路线的签名恢复、设备和商店准备。
2. 本次询问已获用户单独授权并发送成功，回执见第4／6节；**不要再发送相同询问**。
3. 等待并核对原邮件的回复；有新工单号或适用条件时补记，资格明确前不先接SDK。若需跟进，在原线程内按用户当前授权处理，不把本次发送扩展成自动反复发信。
4. 如Mintegral否定或无明确适用答复，再由用户选择第二候选核实或新增Google Play路线。
