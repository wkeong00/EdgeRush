# Edge Rush developer website — GitHub Pages preparation

Prepared on 2026-11-15; deployed and checked on **2026-11-16**. The free GitHub Pages site is live at **https://wkeong00.github.io/EdgeRush/**. [Workflow 43888961128](https://github.com/wkeong00/EdgeRush/actions/runs/43888961128) completed successfully from commit `53f17350fa035bbf900bc75bf4556c065bab053f`; the public deployment status returned that exact URL. No new hosting account, repository, or paid domain was required.

Anonymous HTTPS checks returned **200** for `index.html`, `privacy.html`, `support.html`, `assets/style.css`, `assets/favicon.svg`, and `assets/road-preview.svg`. This proves public delivery, not browser visual QA. Public links: [privacy policy](https://wkeong00.github.io/EdgeRush/privacy.html) and [support](https://wkeong00.github.io/EdgeRush/support.html).

The English site is buildless HTML/CSS. Publishable files are contained in `dist/`:

- `index.html`: Gizmobyte / Thingamajig, the game, and its current internal beta status.
- `privacy.html`: Current beta data practices, simulated reward ads, support email, website hosting, and deletion instructions.
- `support.html`: Public support address `wkeongg@gmail.com`, useful bug-report details, and FAQs.
- `assets/`: Local CSS and original SVG artwork. The road image is explicitly labeled an illustration, not a gameplay screenshot.
- `.nojekyll`: Enables ordinary static-file publishing without Jekyll processing.

No dependencies, JavaScript, forms, external images/fonts, analytics, cookies, browser storage, or advertising tags are included. Ordinary links to external privacy/help pages and `mailto:` links work only when a visitor follows them. There is no invented store URL, download button, testimonial, live-ad claim, or APK distribution endpoint.

## Local preview

From the EdgeRush repository root, run:

```powershell
python -m http.server 8765 --bind 127.0.0.1 --directory docs/site/dist
```

Open `http://127.0.0.1:8765/`. Stop the server with Ctrl+C. No install or build step is needed. Relative links also support a project-site base path such as `/edge-rush/`.

The website files are ready for the repository's GitHub Pages workflow. No Sites hosting project or deployment project ID is used for this route. A prepared local site is not a published website; record the actual deployment URL only after GitHub reports success.

## Source facts used for the privacy draft

The policy is based on code/configuration read on 2026-11-15, rather than an Android network audit.

| Evidence | What it establishes |
| --- | --- |
| [SaveSystem.cs](../../Assets/Scripts/Core/SaveSystem.cs) | Local progress, daily dates, session counters, settings, and reward receipts; app storage plus a PlayerPrefs backup. No remote save endpoint. |
| [Analytics.cs](../../Assets/Scripts/Systems/Analytics.cs) | Recent events in a capped in-memory list and `Debug.Log`; no analytics backend integration in this facade. Logs can contain scores, reward details, coin balances, and random action identifiers. |
| [Ads.cs](../../Assets/Scripts/Systems/Ads.cs) | Editor/Development builds can select the in-game MockRewarded provider. Its TEST AD screen simulates outcomes locally. The conditional AdMob adapter is code scaffolding; this is not a connected Unity Ads/LevelPlay integration. |
| [manifest.json](../../Packages/manifest.json), [packages-lock.json](../../Packages/packages-lock.json) | No Unity Ads, LevelPlay, Google Mobile Ads, or external analytics SDK package is currently installed through these manifests. Source/plugin folders were also searched for an active network integration. |
| [UnityConnectSettings.asset](../../ProjectSettings/UnityConnectSettings.asset) | Unity services, Unity Analytics, cloud diagnostics, performance reporting, and Unity Ads are disabled in the checked project settings. |
| [ProjectSettings.asset](../../ProjectSettings/ProjectSettings.asset) | `submitAnalytics: 1` still exists as an engine setting. Do not infer a verified no-network/no-collection APK from disabled cloud-service settings. Final engine diagnostics behavior requires review. |
| [GameApp.cs](../../Assets/Scripts/Core/GameApp.cs), [Progression.cs](../../Assets/Scripts/Systems/Progression.cs) | Random run/reward identifiers and economic receipts support duplicate-reward prevention; there is no game login or remote account. |

The site does **not** make a blanket claim that Unity, Android, device providers, distributors, email providers, or a future advertising SDK collect no data. Gmail support processing and conditional GitHub Pages IP logging are disclosed separately from local game storage.

## Selected free host — GitHub Pages

The user has chosen free hosting. Use the existing public game repository and the prepared [Publish developer website workflow](../../.github/workflows/developer-site.yml). Its upload path is **only `docs/site/dist`**; the site's 7 files are the deployment artifact. The repository's other documents, code, APKs, signing material, and local files are not uploaded as website content. This does not change the visibility of files already in the public GitHub repository.

1. Push the website files and workflow to `main` after the normal source review. No paid domain or new repository is needed. GitHub Pages is available with GitHub Free for public repositories. [Official overview](https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages)
2. The user has completed [EdgeRush → Settings → Pages](https://github.com/wkeong00/EdgeRush/settings/pages) → **Build and deployment → Source: GitHub Actions**. Do not select the full `/docs` folder as a branch publishing source. [Publishing-source options](https://docs.github.com/en/pages/getting-started-with-github-pages/configuring-a-publishing-source-for-your-github-pages-site)
3. Pushes to `main` that change `docs/site/dist/**` or the website workflow trigger deployment automatically. **Actions → Publish developer website → Run workflow → main** is also available. If GitHub requests environment approval or reports a configuration error, resolve that actual requirement before retrying rather than reporting the site as live.
4. Wait for the **Deploy** step to succeed and record GitHub's returned HTTPS URL. Verify the home, privacy, support, assets, fragment links, mobile layout, and email links without signing in. GitHub supports HTTPS for its default Pages domains. [HTTPS](https://docs.github.com/en/pages/getting-started-with-github-pages/securing-your-github-pages-site-with-https)
5. Public access has been verified, and `BuildInfo.PrivacyPolicyUrl` now points to the published privacy page. Store fields are not yet filled. Before real advertising is added, update the current internal-beta policy to match the chosen SDK and actual data handling.

This informational project/support site includes no purchases, payment processing, or commercial SaaS. Review GitHub's hosting usage limits before expanding its purpose. [Pages limits](https://docs.github.com/en/pages/getting-started-with-github-pages/github-pages-limits)

GitHub documents that Pages logs visitor IP addresses for security; the public privacy page already discloses this conditionally. If another host is selected, update the hosting paragraph to reflect the actual provider before publishing. [Pages data collection](https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages#data-collection)

## Privacy work before a public release

The current text is an internal beta policy. It must be checked against the **actual distributed APK**, not just these source files.

- Decide whether the first public build has no ads or a real approved provider. Do not describe network-served test ads as the current local mock. A real SDK may process data even in its test mode.
- Before shipping any Unity Ads/LevelPlay or other SDK, record exact SDKs/adapters, device and advertising identifiers, IP/device/app/event data, recipients, purposes, retention information and privacy choices from the actual integration and current official provider policies. Update the policy and store declarations; add the appropriate notices/consent controls before initialization. Production ad eligibility and payout verification are separate questions.
- Review `submitAnalytics`, Unity runtime diagnostics, final manifest permissions, Android backup/restore behavior, and network behavior on a real device. Confirm or revise the local-only game-code description. A successful compile or absence of an SDK in UPM is not a network audit.
- Confirm the public publisher/developer identity, support-mailbox access and support-message retention/deletion process. Do not invent a legal company registration, physical address, or fixed response/retention promise.
- Confirm the audience, test conditions, and required age/privacy handling for the actual release. The current test's adult-tester wording is not an age-verification implementation or a store rating.
- Recheck Android deletion steps on the release device, including local save backup and possible operating-system restore. The current UI has no in-game account-deletion or save-reset button; the policy uses Android app-storage controls. [Android storage help](https://support.google.com/android/answer/15728899?hl=en)
- When the public store listing is verified, add its real link and update the internal-beta/download status as appropriate. Do not change simulated-ad wording until the actual distributed implementation changes.
- Confirm the final hosting provider, revise policy scope/date when necessary, and verify the published policy URL without authentication before updating app/store metadata.

## Validation notes

Offline checks are recorded in [2026-11-16-offline-validation.md](2026-11-16-offline-validation.md). **Browser visual QA is not complete**: desktop/narrow layouts, keyboard focus, FAQ disclosure controls, and long email wrapping still need inspection. The local HTTP checks do not prove public hosting or authenticate the future deployment URL. This website review does not validate Android functionality, live advertising, payment eligibility, or legal compliance of a future release.
