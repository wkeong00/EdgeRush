# Website offline validation — 2026-11-16

Scope: the current `docs/site/dist` source, checked with Python's standard HTML and XML parsers. No browser automation or public deployment is represented by this report.

- HTML pages checked: **3**; each has English language metadata, a title and viewport metadata.
- Local links, asset references and HTML fragments checked: **47**; missing targets: **0**.
- SVG XML documents parsed: **2**; errors: **0**.
- Script tags: **0**; remote `src` assets: **0**.
- Files in the deployment directory: **7**. The workflow uploads this directory only.
- Repository diff whitespace check passed separately.

These checks establish source consistency. They do not prove desktop/mobile layout, keyboard focus, FAQ interaction, public HTTPS access, Android gameplay, live advertising, or legal adequacy of a future SDK policy. Browser visual QA remains incomplete because computer control could not verify the browser URL. Public deployment and HTTP checks are recorded in the website README after the actual deployment.
