# Offline compile checks

For the actual Unity Editor release-setting checks, open this project with its pinned editor and installed Pipeline package. From the repository root, with Unity CLI on PATH:

```powershell
unity command script eval --code 'EdgeRush.EditorTools.ReleaseSetupChecks.Run(); EdgeRush.EditorTools.ReleaseBuildRegressionChecks.Run(); return "PASS";' --project-path (Get-Location).Path --caller plugin --skill unity-cli --format json
```

The equivalent Editor menus are **EdgeRush → Checks → Release Setup Preservation** and **Release Configuration Guards**. Run outside Play Mode. The checks temporarily set fictitious release fields and scene registrations, then restore originals in `finally`; they do not build an APK, install ad SDKs or publish. For a separate batch Editor, the executeMethod entry is `EdgeRush.EditorTools.ReleaseSetupChecks.RunBatch`; it exits that Editor after reporting the result. Never invoke RunBatch in a live Editor you intend to keep open.

## Lightweight offline alternative

`compilecheck.ps1` is the repository copy of the original sibling `_tools/compilecheck.ps1`. The original is left untouched. This copy resolves the project relative to the script, so the shell's current directory does not determine which project is checked.

From the EdgeRush repository root, run these checks in order:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\compilecheck.ps1 -Development
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\compilecheck.ps1 -Editor
```

To check the runtime's non-Development conditional branch separately:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\compilecheck.ps1
```

From another directory, pass the absolute path to this script. The default project remains the EdgeRush folder containing `tools/`; `-ProjectRoot` can select another checkout explicitly. The script reads the pinned version from `ProjectSettings/ProjectVersion.txt` and uses that editor's bundled .NET runtime, Roslyn compiler, Unity reference assemblies and uGUI sources. It does not download dependencies or launch an editor.

If Unity is installed outside the standard Windows Unity Hub directory, add `-UnityDataPath` with the absolute path to that installation's `Editor\Data` folder. Use the version pinned by this project. The current define set targets the existing Unity 6000.6 / Windows managed-code baseline; changing Unity version or project packages requires reviewing this script as well.

Generated response files and DLLs go into the ignored `Temp/CompileCheck/Runtime`, `Development` and `Editor` folders. They never use or overwrite the old `_tools/.cc` files. Editor checks always reference `Development/UnityEngine.UI.dll`, because the verification helpers reference development-only Mock types. Rebuild `-Development` after runtime source changes before running `-Editor`; the default runtime output is independent and cannot silently replace that reference.

## What a successful check proves

The selected C# sources compile against the local Unity assemblies. The check preserves the original lightweight approach: runtime game code and most built-in uGUI sources share a temporary assembly, while the unused `InputField.cs` and `DefaultControls.cs` files are excluded. Editor helper sources are compiled separately against that runtime assembly. Exit code 0 indicates compilation success; existing compiler warnings remain visible.

This is not the Unity asset/package pipeline: it does not import assets, execute regression assertions or Play Mode, validate layouts or touch, run a pre-build hook, build an APK/AAB, or prove a release package can be published. Running without `-Development` checks the managed fallback branch only; it is not Android Release validation.

## Real advertising SDK and Android boundary

These offline modes define `UNITY_STANDALONE_WIN`; they do **not** define `UNITY_ANDROID`, `UNITY_IOS` or `EDGERUSH_ADMOB`. Consequently, a successful check does not compile or validate the conditional Google Mobile Ads adapter, Android-specific haptics, UMP, Gradle dependencies, Android manifests, native libraries, API targets, signing, or actual ad callbacks. The SDK-free fallback and Mock paths must not be reported as production SDK validation.

After choosing and installing the real SDK, lock its compatible version and dependencies, configure the correct App ID and enabled unit IDs, and enable the provider's define in the actual Unity target configuration. Then compile/build the Android target with the pinned Unity editor, verify the production pre-build guard, and test privacy, rewarded events, lifecycle and audio on physical Android devices using test ads. Record those results separately from these offline checks. This tool deliberately does not install SDKs, change provider defines or alter release identities/signatures.

## Preparing a new Android release signing key

`new-android-signing-key.ps1` is an interactive local helper for a **new** release signing key. Before using it, confirm that Edge Rush has no previously used release signing key that must be retained. A search of the workspace and common Android directories found no key; that does not prove there is no old key elsewhere. Do not replace an existing published app's signing key with this helper.

Read-only preflight, from this repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\new-android-signing-key.ps1 -ValidateOnly
```

After confirming a new key is appropriate, run the same command without `-ValidateOnly` **yourself in a local terminal**. Type the requested confirmation, then enter a new password twice using the hidden prompts. Do not paste the password into chat or a command argument. The script defaults to `UserSettings/Signing/edgerush-release.keystore`, alias `edgerush`, PKCS12, RSA 4096, and 10,000 days. `-KeySize 2048` is also supported. The certificate uses only the already public publisher label `CN=Gizmobyte, O=Gizmobyte, C=MY`; this is not a company-registration verification.

`-ExecutionPolicy Bypass` above is limited to that new PowerShell process so this reviewed local helper can run; it does not change the saved user or machine execution policy. Do not run `Set-ExecutionPolicy` or relax a managed policy to use this helper.

The script uses the pinned Editor's Android OpenJDK; pass `-UnityDataPath` only if that Editor is installed elsewhere. `-FileName` accepts a filename, not an arbitrary path. Existing files, signing-path junctions/symbolic links, paths outside `UserSettings/Signing`, and outputs not ignored by Git are refused. Preflight does not create directories, generate keys, or prompt for a password.

The password is passed using keytool's `-storepass:env` / `-keypass:env` only in each child process environment. It is not put in process arguments, output, a file, or the machine/user environment. Plaintext necessarily exists briefly in process memory for keytool; the helper disposes secure inputs and clears its unmanaged password buffers afterward. It does not read existing credentials or persist the password. The same password protects the store and key for PKCS12 compatibility.

On success, only the path, alias and public SHA-256 fingerprint are returned. A temporary encrypted keystore is moved to the final filename with an operation that refuses overwrite. If generation or verification fails after writing a temporary key, that uniquely named file is retained for local review, not deleted automatically.

Back up the encrypted keystore and password in separate secure locations and verify a restore locally. Never commit the key or password, send them in chat, or rely on a copy on the same drive as the only backup. This helper does not configure Unity signing, register a package, prove backup recovery, build a signed APK, or complete checklist I06/I07. Public certificate fingerprints may be recorded after actual generation; no new signing key has been generated merely by adding or validating this script.

Reference: [Oracle keytool documentation](https://docs.oracle.com/en/java/javase/17/docs/specs/man/keytool.html) for `-genkeypair`, password environment modifiers, PKCS12 compatibility and certificate listing.
