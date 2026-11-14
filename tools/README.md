# Offline compile checks

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
