# Edge Rush · TapTap 国际版上架资料草稿

> 整理／官方资料核查：2026-11-15（Asia/Singapore）。
>
> 用途：对应 [执行清单 P9](07-TapTap国际版上线与广告盈利执行清单.md) 的文案、字段和素材准备。本次只形成草稿，没有创建商店条目、上传素材、提交审核或发布。
>
> 当前路线：先发 TapTap 国际版 Android APK，英文、马来西亚小范围试玩；普通大众、不针对儿童，首轮招募成年测试者。广告平台接入方式与正式投放资格仍在核对，真实广告 SDK 未安装。正式图标、封面、真实 Android 截图／视频及可发布 APK 均未交付。

## 1. 后台回填卡

以下是拟填写值，不代表后台已经保存。字段名称／是否提供独立字段，以 `developer.taptap.io` 当前表单为准。

| 字段 | 拟填写值 | 状态／回填说明 |
| --- | --- | --- |
| Game title | `Edge Rush` | 与工程产品名一致；不加国家、广告词或热门关键词后缀 |
| Developer／开发者账号 | `Thingamajig` | 用户提供；TapTap ID `799998484`，不是游戏条目 ID |
| Provider／Publisher／厂商 | `Gizmobyte` | 用户提供，已用于 Player Settings 公司名；确认后台厂商字段绑定正确账号 |
| Support email | `wkeongg@gmail.com` | 用户指定可公开邮箱；填联系字段，不印入封面／截图 |
| Android package | `com.gizmobyte.edgerush` | 工程拟定值；首次上传前核对后台条目、实际 APK 与可用性，尚未确认占用通过 |
| Version name／code | `0.1.0`／`1` | 当前源码配置；提交时从最终 APK 回读，不能用此表代替产物核验 |
| Platform／orientation | Android／Portrait | 当前实现；系统兼容信息按最终 APK 和真机结果回填 |
| Language | English | 英文游戏文案与英文商店素材；不宣称已有中文或马来文支持 |
| Region | Malaysia | 首轮拟定区域；未列地区是否仅能关注、是否允许下载，以后台设置与实际访问核验 |
| Genre／tags | Arcade；Driving／Racing；Single-player | 文案建议；只选后台实际提供且与游戏相符的分类，不宣称多人竞速或车辆模拟 |
| Price | Free | 当前免费游玩路线；虚拟金币不是现金奖励 |
| In-app purchases | 当前未实现真实货币内购 | 最终包再核对；不要把金币买外观当成真钱内购 |
| Advertising | 待最终 Provider 与 APK 确认 | 当前有激励广告流程代码，正式 SDK／广告资格尚未验证；不能提前把广告版声明填写完毕 |
| Audience／rating | 普通大众，不针对儿童；年龄分级待实际问卷 | 成年测试者安排不是“18+ 内容”分级依据；不预填未经核实的年龄标签 |
| Website／privacy policy | [网站](https://wkeong00.github.io/EdgeRush/)／[隐私](https://wkeong00.github.io/EdgeRush/privacy.html)／[支持](https://wkeong00.github.io/EdgeRush/support.html) | 2026-11-16 部署及匿名HTTP访问通过；政策当前为内部Mock测试版，最终SDK接入后必须复核，商店字段尚未回填 |
| Login／progress | 当前玩法无需账号登录，本机存档 | 不宣称云存档、跨设备同步或在线排行榜；按候选包复核 |
| Store status／release date | 保持草稿；日期待验收 | 不因资料齐全就选择已开放下载，也不先承诺上线日 |
| Game page URL／game ID | **待后台实际条目** | 先确认是否已有 Edge Rush 条目；避免重复创建。开发者 ID 不填入此栏 |

创建后仍需完成资料并提交审核；区域和语言应在条目内明确配置。此稿尚未操作这些步骤。[国际版创建与发布](https://developer.taptap.io/docs/store/store-creategame/)

## 2. 可复制的英文商店文案

### 2.1 短简介

```text
Dodge traffic, chain close calls, and chase your best score on a neon highway.
```

共 **78 个 ASCII 字符（含空格和标点）**。这是内部短版，方便后台存在摘要字段时使用；已查官方页没有给出独立短简介字段或 80 字限制，不能把此长度当成 TapTap 平台规定。若只有 Description，可直接使用下方长简介。

### 2.2 Description／About

以下英文共约 142 词，复制代码块中的内容；各行之间没有空白行。官方素材页要求 About 避免空白行，以免客户端显示异常。[国际版文字素材](https://developer.taptap.io/docs/store/store-material/#description)

```text
One last-second lane change can turn a close call into your next high score. Edge Rush is a portrait arcade driving game built around three lanes, quick decisions, and a neon highway.
Tap the left or right side of the screen, or swipe, to change lanes. Read the traffic and move out just in time. Keep close calls coming to raise their bonus up to x5, then see how far you can take your run.
Collect coins and unlock new car looks in the garage. Drive through a neon city, a tunnel, and desert dusk as the road changes around you. Daily rewards and missions add more goals to work toward.
Learn by doing in a short, replayable tutorial. Adjust music, sound effects, vibration, and reduced effects in Settings. Your personal best is saved on this device, ready for your next attempt.
```

此版本没有加入广告、全部七车可解锁、离线保证或性能承诺。提交前应在真实候选 APK 中复核上述功能，不把源码存在直接写成 Android 已验收。

### 2.3 卖点短句备选

这些是简介／测试帖的备选文字；**封面和推广图只放游戏标题，不把短句印在图上**。

| 英文短句 | 对应玩家价值 |
| --- | --- |
| Three lanes. One last-second move. | 三车道、单指变道 |
| Chain close calls. Build your bonus to x5. | 擦边连击；倍率用于擦边奖励 |
| Collect coins. Find your next car look. | 金币与车库外观目标 |
| Learn by doing, then chase your personal best. | 实操教学、个人纪录 |

### 2.4 Notes／测试说明草稿

如果后台提供独立 Notes／测试说明，可在确认测试安排后使用下段。它描述测试目的，避免重复 About 的玩法介绍。

```text
This early Android test focuses on controls, traffic readability, and menu layout. English-speaking adult testers in Malaysia are invited to share feedback through TapTap. When reporting a problem, please include your device model, Android version, and the steps that led to it. Progress is stored on this device; clearing app data or reinstalling may remove it.
```

只在实际开启对应测试、确定反馈入口后使用，不把这段当作已经开放下载的公告。支持邮箱填专门的联系字段；简介与素材不加入外部导流链接。About 与 Notes 应各司其职。[国际版审核规则 §3.5–3.7](https://developer.taptap.io/docs/store/store-agree/#35-text-materials-that-you-provide)

### 2.5 What's new／更新说明

**首次发布或预约：按官方指引留空，不填“Initial release”来凑字段。** 如果实际后台强制必填，先核对当前提示，不擅自照旧规则提交。[国际版更新说明要求](https://developer.taptap.io/docs/store/store-material/#whats-new)

下面只预备给**未来真正的更新版本**，并非当前首发可直接粘贴的内容。只有相较已发布版本确实包含这些改动、并经 Android 验收后，才保留相应条目；版本号与最终包另行回填。

```text
Improved tap and swipe input handling.
Refined menu and garage layouts for different screen shapes.
Added a replayable hands-on tutorial.
Improved daily reward cycling and local reward accounting.
```

没有旧公开版本可比较时，这些属于内部开发改动，不包装为已上线游戏的更新。后续修改可在 Store Info 的新版本中准备草稿，提交前检查继承的旧素材是否仍准确。[国际版更新与草稿](https://developer.taptap.io/docs/store/store-update/)

## 3. 文案与实现对照

| 可核实内容 | 源码依据 | 对外表述边界 |
| --- | --- | --- |
| 三条车道、点击左右半屏或滑动变道 | [Layout.cs](../Assets/Scripts/Core/Layout.cs)、[GameApp.cs](../Assets/Scripts/Core/GameApp.cs)、[PlayerCar.cs](../Assets/Scripts/Gameplay/PlayerCar.cs) | 可以说 three lanes、tap／swipe；不宣称方向盘操控、开放世界或多人比赛 |
| 擦边连击最高 x5 | [RunSession.cs](../Assets/Scripts/Gameplay/RunSession.cs) 的 `AwardNearMiss`；[Tuning](../Assets/Scripts/Core/Layout.cs) 的 `ComboMax = 5` | x5 用于擦边积分；距离分和金币分没有一并乘五，故不写“所有得分五倍” |
| 七个外观定义 | [Palette.cs](../Assets/Scripts/Art/Palette.cs) 的 `Skins.All` | 1 款初始车＋5 款金币车＋1 款广告车；不是七款均已完成正式解锁验收 |
| Chrome Ghost 为广告车 | 同上 `ghost.adUnlock = true`；[Progression.cs](../Assets/Scripts/Systems/Progression.cs)；`SkinAdCount = 5` | 需要累计奖励广告进度；当前真实广告未接通，主简介不承诺“解锁全部七款” |
| 外观更换不改变操作规则 | [PlayerCar.cs](../Assets/Scripts/Gameplay/PlayerCar.cs) 的 `Setup`／`Tick`；公共 Tuning | 可以称 car looks；不写更快、加速能力、更小碰撞箱或高级车属性优势 |
| City／Tunnel／Desert Dusk 三主题 | [Palette.cs](../Assets/Scripts/Art/Palette.cs) 的 `Themes`；[RunSession.cs](../Assets/Scripts/Gameplay/RunSession.cs) | 主题随跑局距离切换；不是三个独立可选关卡或地图模式 |
| 金币、签到、每日任务、本机纪录 | [Progression.cs](../Assets/Scripts/Systems/Progression.cs)、[SaveSystem.cs](../Assets/Scripts/Core/SaveSystem.cs) | 不写线上赛事、云同步、玩家全球排名或现实收益 |
| 左右变道与擦边实操教学，可跳过／重播 | [TutorialSession.cs](../Assets/Scripts/Gameplay/TutorialSession.cs)、[GameApp.cs](../Assets/Scripts/Core/GameApp.cs) | 可以写 replayable tutorial；不宣称它已通过 Android 触摸验收 |
| 音乐、音效、震动、减弱效果设置 | [MenuScreens.cs](../Assets/Scripts/UI/MenuScreens.cs)、[UiRoot.cs](../Assets/Scripts/UI/UiRoot.cs) | 描述现有选项；不宣称无光敏风险、所有特效完全关闭或设备全覆盖 |
| 复活／结算翻倍等广告流程壳 | [Ads.cs](../Assets/Scripts/Systems/Ads.cs)、[manifest.json](../Packages/manifest.json) | 真实广告 SDK 未安装，Provider 与资格仍待定；不宣传“正式激励广告已可用” |

磁铁、护盾、冲刺、慢速道具、在线排行榜等仍不进入本版文案或素材。也不添加“已上线”“稳定 60fps”“零卡顿”“已有玩家留存／收入”等没有对应证据的说法。

## 4. 官方素材规格与本项目交付表

本表按 **TapTap 国际版**公开文档整理；提交前再对照当前上传控件，不套用中国大陆文档或 Google Play 的尺寸／数量限制。[国际版素材要求](https://developer.taptap.io/docs/store/store-material/)

| 素材 | 官方公开要求／建议 | 本项目拟交付与当前状态 |
| --- | --- | --- |
| 商店 Icon | 512×512；JPG／PNG；重点按官方示意留安全区 | **候选已制作**：[icon-512.png](store-assets/icon-512.png)，不透明青色车辆图；已导入Unity默认图标，APK／设备与后台裁切仍待 |
| 详情页顶部 Cover | 必填；1920×1080；JPG／PNG；主体／标题在 1760×920 安全区内 | **候选已制作**：[cover-1920x1080.png](store-assets/cover-1920x1080.png)，独立品牌插画，仅Edge Rush标题；主要车与标题边界已核对安全区 |
| 游戏 Screenshots | 同尺寸 JPG／PNG；官方建议至少 3 张；竖屏推荐 9:16 | **缺件**：拟 5 张真实候选 APK 截图，统一 1080×1920；这属于制作目标，不是官方强制尺寸 |
| Gameplay video | 可选；MP4；≤1GB、≤60 秒；建议约 20 秒；≥1280×720，建议 1920×1080 | **缺件／可后补**：拟 20 秒真实 Android 录屏剪辑；不生成伪玩法视频 |
| Video cover | 上传视频时需要；1256×706；JPG／PNG | **缺件／随视频**：与最终视频内容一致的封面 |
| Promotion image | 正文称可选；1920×1080；JPG／PNG；只放游戏标题 | **缺件／可后补**：标题带 `*` 与正文可选表述不一致，以当前后台是否必填复核 |
| 权利／授权材料 | 涉及授权时提供对应证明 | 待整理程序美术／合成音乐来源记录和实际采用素材的权利证明；不虚构已取得第三方授权 |

源页竖屏截图行同时列出“9:16”和“1280×720 以上”，方向表述不够清楚；因此先拟定常用竖屏拍摄目标，**最终按上传校验复核**，不为满足横向数字而拉伸画面。源页没有列出统一的图标／截图文件大小上限，本稿不自行写成 2MB。若视频需横版成片，应完整保留真实竖屏画面并制作有内容的品牌背景，避免拉伸或黑边。

封面／推广图须独立制作，不用玩法截图、截图拼贴、手机外框或游戏 UI 充当；只保留标题文字。截图则应忠实呈现最终下载版本，去掉设备系统 UI，不伪造分数或改出不存在的功能。[国际版审核规则 §3.1–3.4](https://developer.taptap.io/docs/store/store-agree/#31-specifications)

### 4.1 建议实际截图清单

以下是拍摄任务，当前没有可上传文件。文件名仅作规划，不表示素材已经存在。

| 顺序／拟文件名 | 真实画面 | 取景验收点 |
| --- | --- | --- |
| 1 · `01-gameplay.png` | 正式跑局，玩家车与前方交通 | 能看清三车道、障碍与实际操作空间；不摆造假交通 |
| 2 · `02-close-call.png` | 实际达成的擦边／连击 | 只用正常操作获得的提示、倍率与分数；没打到 x5 就拍真实已达倍率 |
| 3 · `03-garage.png` | 正式包车库与实际拥有／可购买外观 | 不用修改存档解锁广告车，不暗示未接通的广告路径已经可用 |
| 4 · `04-results.png` | 实际结束一局的结算 | 分数、擦边数、最高倍数和金币均来自该局 |
| 5 · `05-tutorial-or-missions.png` | 实际教学步骤或任务页 | 文案清楚、无裁切；任务进度使用正常存档 |

最终相册的尺寸／方向保持一致。系统状态栏、通知、账号信息及调试菜单不进入成片。每份原始录屏或截图记录候选包版本、commit、设备型号、Android 版本、分辨率与拍摄时间；保留原件，导出后检查清晰度及上传显示。

历史 `docs/art-preview-*.png` 是离线美术示意；`docs/verification-ab/` 是 Editor 验收素材，部分页面使用构造的分数／金币。**两者均不直接用作正式商店截图或真实 Android 玩法证明。** 新的 [品牌素材](store-assets/README.md) 是独立原创插画，也不能作为玩法截图；图标／封面候选已制作，但未上传后台。

## 5. 提交前缺件表

| 项目 | 当前状态 | 需要补齐的结果 |
| --- | --- | --- |
| 英文文案 | 本稿已准备 | 用户确认语气；与最终候选 APK 逐句核对 |
| 游戏条目及名称／包名可用性 | 未核实 | 实际条目 ID／URL、正确厂商绑定、首传包名核对 |
| Icon／Cover | 候选已完成，后台与设备待验 | PNG／对应SVG／可重现源码和来源已交付，根代理审图；Unity默认图标已设，安装包实际图标与后台裁切仍待核对 |
| 真实 Android 截图 | 缺件 | 至少准备官方建议的 3 张，按上表拟拍 5 张；带来源记录 |
| 真实录屏及视频封面 | 缺件，视频选填 | 若本轮使用视频则一并交付；不因此伪造素材 |
| 发布 APK | 未交付 | 正式签名、实际包名／版本、安装／升级与真机流程通过，记录产物校验值 |
| 政策和支持网站 | 已公开；最终SDK政策仍待 | 当前链接已匿名访问通过，工程隐私URL已替换；真实SDK接入后更新数据披露／选择，商店字段仍待回填 |
| 广告／数据声明 | 未定稿 | Provider、最终 SDK、实际权限／数据／广告范围与政策、后台一致 |
| 受众／区域／测试状态 | 方向已确认，后台未回填 | Malaysia、English、普通大众且不针对儿童；实际分级问卷和发布状态待填 |
| 权利记录 | 待整理 | 仅对实际采用的美术、字体、音频、工具输出整理出处／授权依据 |
| 审核／公开下载 | 尚未进行 | 完整草稿与候选包先可核对，再按授权提交；记录平台反馈和实际下载结果 |

## 6. 本稿完成后的回填顺序

1. 核对已有 Edge Rush 条目和厂商账号；保留本地草稿，不重复创建页面。
2. 最终 SDK／政策与正式候选包完成后，重新审查广告、受众、权限和数据字段。
3. 真机完成跑局并拍摄素材，独立制作图标／封面；逐张检查上传效果。
4. 将确认后的字段和英文文案填入后台草稿；首发 What's new 按当前规则处理。此步尚未执行。
5. 对照候选 APK、区域／状态、政策链接、素材和版本，再准备提交审核；记录结果，不承诺固定审核时长或开放日期。

## 7. 本次核查来源

- [TapTap 国际版：Create And Release Games](https://developer.taptap.io/docs/store/store-creategame/)：创建、条目状态、区域和语言、提交流程。
- [TapTap 国际版：Assets Requirements](https://developer.taptap.io/docs/store/store-material/)：Icon、文字、截图、视频及两类封面规格；首发 What's new 留空。
- [TapTap 国际版：Game Review Specifications and Rules](https://developer.taptap.io/docs/store/store-agree/)：素材真实性、推广图限制、文案与字段一致性、包身份与签名相关检查。
- [TapTap 国际版：Update Game](https://developer.taptap.io/docs/store/store-update/)：后续版本草稿与更新资料流程。
- 工程依据为第 3 节对应源码及 [执行清单](07-TapTap国际版上线与广告盈利执行清单.md) 的用户确认参数；本次只核对源码，没有重新运行 Unity、构建 Android 或测试真实广告。

公开文档与当前后台不一致的项目，保留问题并以实际表单／官方确认更新，不能用国内站规则补作国际版结论。本稿没有保存证件、银行资料、密码或签名私钥。
