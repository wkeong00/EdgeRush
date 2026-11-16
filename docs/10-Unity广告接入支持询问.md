# Unity Ads integration support inquiry

Status: **SUBMITTED — PUBLISHER CONFIRMED; AWAITING REPLY**

Prepared: 2026-11-15

Official recipient: [Unity Ads Support](https://support-ads.unity.com/s/)

Purpose: Confirm the supported production monetization path and eligibility for a new Android app distributed exclusively through TapTap International: standalone Unity Ads, Unity Ads bidding, or LevelPlay.

The publisher explicitly authorized submission of this draft on 2026-11-16, and subsequently confirmed that they successfully submitted the form. No ticket number or support reply has been provided yet. Do not send a duplicate request. No bank, identity, tax, credential, or signing-key data may be attached.

The official [Contact Us form](https://support-ads.unity.com/s/ContactUs) is filled in Chrome with the public developer name/email, the message below and its official source URLs. Product: Unity Ads Monetization; category: Technical issue; sub-category: SDK Integration assistance; Game ID: 800438692; mediation: Other. Both SDK-version fields truthfully state that integration has not started. No file is attached.

The first automated attempt displayed **Please verify you're not a robot**. Computer control later stopped because it could not reliably verify the browser URL. After resuming, the publisher completed the submission themselves and explicitly reported success. This is user-confirmed submission evidence, not an independently captured receipt or a response confirming monetization eligibility. Record any later ticket number/reply below before taking integration decisions.

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

## Official documentation requiring clarification

Sources reviewed on 2026-11-15. These are documentation observations, not confirmation of production eligibility.

- [Unity 6.6 manual: Advertisement Legacy](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.ads.html): Says direct monetization using this package is unsupported from January 31, 2026, and recommends LevelPlay. The same page lists package version 4.19.0 for Unity 6000.6.
- [Unity Ads SDK 4.21.0 integration guide](https://docs.unity.com/en-us/ads-unity/4.21.0/sdk-integration): Says direct integrations may see reduced performance from April 1, 2026, while Unity Ads continues supporting and filling them. It also says mediated integrations should follow their adapter guide to avoid duplicate initialization.
- [New-dashboard placement creation](https://docs.unity.com/en-us/monetization/dashboard/placements/create-manage-placements): States the New Unity Dashboard currently supports mediated users only. [Placement overview](https://docs.unity.com/en-us/monetization/dashboard/placements) says only bidding placements can now be created.
- [Dashboard changes for publishers](https://docs.unity.com/en-us/monetization/dashboard/dashboard-changes): Includes instructions to use placement IDs in app code when not using mediation, which does not make the availability of that path for newly created bidding placements clear.
- [LevelPlay: Add an app](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/add-app): Allows a temporary app entry when an app is not live in the App Store or Google Play. [LevelPlay: Create an account](https://docs.unity.com/en-us/grow/levelplay/platform/get-started/create-account) separately describes account approval and requests for information about published apps. These pages do not explicitly confirm TapTap-only production eligibility.
- [Unity Ads bidder configuration in LevelPlay](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/networks/guides/unity-ads): Documents manual bidder setup with a Game ID and Placement ID. Confirmation is requested that the existing app and placement above can use that route.

## Reply record — complete only after a response

| Item to record | Confirmed answer / reference |
| --- | --- |
| Ticket number, submission date, responding team, and reply date | Publisher confirmed successful submission on 2026-11-16; ticket number, responding team and reply date not yet provided |
| Supported route for this new account: standalone, LevelPlay bidder, or other | Awaiting confirmation |
| Exact supported Unity package ID/version and integration guide | Awaiting confirmation |
| Whether the existing bidding placement is valid for direct load/show | Awaiting confirmation |
| TapTap-only eligibility; accepted listing/review evidence; unpublished-app policy | Awaiting confirmation |
| Store, traffic, Malaysia, and individual-publisher requirements | Awaiting confirmation |
| Reuse of Game ID and placement; any dashboard configuration changes | Awaiting confirmation |
| Outstanding account/app/network approvals and responsible review team | Awaiting confirmation |
| Payment-profile reuse or additional payment onboarding | Awaiting confirmation |
| Test-mode procedure and explicit conditions for enabling live traffic | Awaiting confirmation |
| Official explanation of the conflicting documentation and applicable policy date | Awaiting confirmation |

Record the support team's exact scope and conditions. An integration test, payout verification, or generic statement that Android is supported must not be recorded as approval of TapTap-only production monetization.
