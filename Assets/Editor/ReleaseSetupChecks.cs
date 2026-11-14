using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace EdgeRush.EditorTools
{
    /// <summary>Editor-only regression checks. Temporary probes are always restored, including credentials.</summary>
    public static class ReleaseSetupChecks
    {
        static int _assertions;

        /// <summary>One executeMethod entry point; Run's finally finishes before Editor shutdown.</summary>
        public static void RunBatch()
        {
            int exitCode = 0;
            try
            {
                Run();
                ReleaseBuildRegressionChecks.Run();
                Debug.Log("[Checks] PASS: release setup preservation and release configuration guards; original settings restored.");
            }
            catch (Exception error)
            {
                exitCode = 1;
                // Unity setter exceptions can contain configuration values. Only our fixed
                // assertion labels are safe to print; never log arbitrary exception messages.
                string detail = "Raw configuration and signing credentials omitted.";
                if (error is InvalidOperationException &&
                    (error.Message.StartsWith("Release setup check failed: ", StringComparison.Ordinal) ||
                     error.Message.StartsWith("Release configuration regression: ", StringComparison.Ordinal)))
                    detail = error.Message;
                Debug.LogError("[Checks] FAIL: release checks (" + error.GetType().Name + "). " + detail);
                // Stack frames locate a failing Unity API without exposing its exception payload.
                Debug.LogError(error.StackTrace);
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        [MenuItem("EdgeRush/Checks/Release Setup Preservation")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run release setup checks outside Play Mode.");

            var original = new SettingsSnapshot();
            string folder = "Assets/__ReleaseSetupChecks_" + Guid.NewGuid().ToString("N");
            bool mainExisted = File.Exists(EdgeRushSetup.ScenePath);
            bool scenesFolderExisted = AssetDatabase.IsValidFolder("Assets/Scenes");
            _assertions = 0;
            try
            {
                AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
                string before = folder + "/Before.unity";
                string after = folder + "/After.unity";
                CreateProbeScene(before);
                CreateProbeScene(after);

                // These are deliberately fictitious values, never a publisher identity or real key.
                PlayerSettings.companyName = "Setup Preservation Probe";
                PlayerSettings.productName = "Release Setup Probe";
                PlayerSettings.bundleVersion = "7.8.9";
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.edgerush.setupchecks");
                PlayerSettings.Android.bundleVersionCode = 789;
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = folder + "/verification-only.keystore";
                PlayerSettings.Android.keyaliasName = "verification-only-alias";
                PlayerSettings.Android.keystorePass = "verification-only-not-a-real-secret";
                PlayerSettings.Android.keyaliasPass = "verification-only-not-a-real-secret";
                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)31;
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
                var expected = new SettingsSnapshot();

                // Start with settings unlike the runtime requirements to exercise first-time setup.
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                PlayerSettings.allowedAutorotateToPortrait = false;
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.Mono2x);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(before, false),
                    new EditorBuildSettingsScene(after, true),
                };

                for (int attempt = 0; attempt < 2; attempt++)
                {
                    EdgeRushSetup.Apply(false);
                    expected.AssertReleaseFieldsPreserved();
                    Require(PlayerSettings.Android.minSdkVersion == (AndroidSdkVersions)31, "Higher minimum SDK retained");
                    Require(PlayerSettings.Android.targetSdkVersion == (AndroidSdkVersions)36, "Explicit target SDK retained");
                    AssertSceneList(new[] { before, after, EdgeRushSetup.ScenePath }, new[] { false, true, true });
                    Require(PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait &&
                        PlayerSettings.allowedAutorotateToPortrait && !PlayerSettings.allowedAutorotateToPortraitUpsideDown &&
                        !PlayerSettings.allowedAutorotateToLandscapeLeft && !PlayerSettings.allowedAutorotateToLandscapeRight,
                        "Portrait gameplay configuration applied");
                    Require(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP,
                        "IL2CPP configured");
                    Require(PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64, "ARM64 configured");
                    Require(PlayerSettings.Android.appCategory == "game", "Game application category configured");
                }

                // A registered but disabled Main scene is an explicit build configuration.
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(before, false),
                    new EditorBuildSettingsScene(EdgeRushSetup.ScenePath, false),
                    new EditorBuildSettingsScene(after, true),
                };
                EdgeRushSetup.Apply(false);
                AssertSceneList(new[] { before, EdgeRushSetup.ScenePath, after }, new[] { false, false, true });
                expected.AssertReleaseFieldsPreserved();

                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)23;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
                EdgeRushSetup.Apply(false);
                Require(PlayerSettings.Android.minSdkVersion == AndroidSdkVersions.AndroidApiLevel26,
                    "Minimum SDK floor applied to an older setting");
                Require(PlayerSettings.Android.targetSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto,
                    "Auto target SDK retained");
                AssertSceneList(new[] { EdgeRushSetup.ScenePath }, new[] { true });
                expected.AssertReleaseFieldsPreserved();

                // A boolean legacy snapshot would lose a non-game category during restoration.
                PlayerSettings.Android.appCategory = "audio";
                var nonGameCategory = new SettingsSnapshot();
                EdgeRushSetup.Apply(false);
                Require(PlayerSettings.Android.appCategory == "game", "Game category reapplied");
                nonGameCategory.Restore();
                Require(PlayerSettings.Android.appCategory == "audio", "Full non-game category restored exactly");
            }
            finally
            {
                // Keep all probe credentials in memory; never print or persist them in a report.
                // Restore project settings before removing the assets referenced by the probe list.
                try { original.Restore(); }
                finally
                {
                    if (AssetDatabase.IsValidFolder(folder)) AssetDatabase.DeleteAsset(folder);
                    if (!mainExisted && File.Exists(EdgeRushSetup.ScenePath)) AssetDatabase.DeleteAsset(EdgeRushSetup.ScenePath);
                    if (!scenesFolderExisted && Directory.Exists("Assets/Scenes") &&
                        Directory.GetFileSystemEntries("Assets/Scenes").Length == 0)
                        AssetDatabase.DeleteAsset("Assets/Scenes");
                    AssetDatabase.SaveAssets();
                }
            }

            Debug.Log("[Checks] Release setup preservation passed (" + _assertions + " assertions); original settings restored.");
        }

        static void CreateProbeScene(string path)
        {
            // Duplicate the committed boot scene without opening it. Unity rejects additive
            // NewScene while the user has an unsaved Untitled scene; checks must preserve it.
            Require(File.Exists(EdgeRushSetup.ScenePath), "Committed Main scene available for temporary probes");
            Require(AssetDatabase.CopyAsset(EdgeRushSetup.ScenePath, path), "Temporary scene copied without changing open scenes");
        }

        static void AssertSceneList(string[] paths, bool[] enabled)
        {
            var actual = EditorBuildSettings.scenes;
            Require(actual.Length == paths.Length, "Scene count retained without duplicates");
            for (int i = 0; i < paths.Length; i++)
                Require(actual[i].path == paths[i] && actual[i].enabled == enabled[i], "Scene order and enabled state retained");
        }

        static void Require(bool condition, string message)
        {
            _assertions++;
            if (!condition) throw new InvalidOperationException("Release setup check failed: " + message);
        }

        sealed class SettingsSnapshot
        {
            readonly string _company = PlayerSettings.companyName;
            readonly string _product = PlayerSettings.productName;
            readonly string _version = PlayerSettings.bundleVersion;
            readonly string _package = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            readonly int _versionCode = PlayerSettings.Android.bundleVersionCode;
            readonly bool _customKeystore = PlayerSettings.Android.useCustomKeystore;
            readonly string _keystoreName = PlayerSettings.Android.keystoreName;
            readonly string _keyaliasName = PlayerSettings.Android.keyaliasName;
            readonly string _keystorePass = PlayerSettings.Android.keystorePass;
            readonly string _keyaliasPass = PlayerSettings.Android.keyaliasPass;
            readonly AndroidSdkVersions _minSdk = PlayerSettings.Android.minSdkVersion;
            readonly AndroidSdkVersions _targetSdk = PlayerSettings.Android.targetSdkVersion;
            readonly UIOrientation _orientation = PlayerSettings.defaultInterfaceOrientation;
            readonly bool _portrait = PlayerSettings.allowedAutorotateToPortrait;
            readonly bool _upsideDown = PlayerSettings.allowedAutorotateToPortraitUpsideDown;
            readonly bool _landscapeLeft = PlayerSettings.allowedAutorotateToLandscapeLeft;
            readonly bool _landscapeRight = PlayerSettings.allowedAutorotateToLandscapeRight;
            readonly bool _playerLog = PlayerSettings.usePlayerLog;
            readonly ColorSpace _colorSpace = PlayerSettings.colorSpace;
            readonly ScriptingImplementation _backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
            readonly AndroidArchitecture _architectures = PlayerSettings.Android.targetArchitectures;
            readonly bool _internet = PlayerSettings.Android.forceInternetPermission;
            readonly bool _sdCard = PlayerSettings.Android.forceSDCardPermission;
            readonly bool _split = PlayerSettings.Android.splitApplicationBinary;
            readonly bool _minify = PlayerSettings.Android.minifyRelease;
            readonly string _appCategory = PlayerSettings.Android.appCategory;
            readonly bool _defaultGraphics = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
            readonly GraphicsDeviceType[] _graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            readonly EditorBehaviorMode _behavior = EditorSettings.defaultBehaviorMode;
            readonly SerializationMode _serialization = EditorSettings.serializationMode;
            readonly EditorBuildSettingsScene[] _scenes = EditorBuildSettings.scenes;
            readonly bool _hasSetupPref = EditorPrefs.HasKey(EdgeRushSetup.PrefKey);
            readonly bool _setupPref = EditorPrefs.GetBool(EdgeRushSetup.PrefKey, false);

            public void AssertReleaseFieldsPreserved()
            {
                Require(PlayerSettings.companyName == _company, "Company retained");
                Require(PlayerSettings.productName == _product, "Product name retained");
                Require(PlayerSettings.bundleVersion == _version, "Version retained");
                Require(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) == _package, "Package identifier retained");
                Require(PlayerSettings.Android.bundleVersionCode == _versionCode, "Version code retained");
                Require(PlayerSettings.Android.useCustomKeystore == _customKeystore, "Custom signing retained");
                Require(PlayerSettings.Android.keystoreName == _keystoreName, "Keystore reference retained");
                Require(PlayerSettings.Android.keyaliasName == _keyaliasName, "Signing alias retained");
                Require(PlayerSettings.Android.keystorePass == _keystorePass && PlayerSettings.Android.keyaliasPass == _keyaliasPass,
                    "Signing credentials retained");
            }

            public void Restore()
            {
                PlayerSettings.companyName = _company;
                PlayerSettings.productName = _product;
                PlayerSettings.bundleVersion = _version;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, _package);
                PlayerSettings.Android.bundleVersionCode = _versionCode;
                PlayerSettings.Android.useCustomKeystore = _customKeystore;
                PlayerSettings.Android.keystoreName = _keystoreName;
                PlayerSettings.Android.keyaliasName = _keyaliasName;
                PlayerSettings.Android.keystorePass = _keystorePass;
                PlayerSettings.Android.keyaliasPass = _keyaliasPass;
                PlayerSettings.Android.minSdkVersion = _minSdk;
                PlayerSettings.Android.targetSdkVersion = _targetSdk;
                PlayerSettings.defaultInterfaceOrientation = _orientation;
                PlayerSettings.allowedAutorotateToPortrait = _portrait;
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = _upsideDown;
                PlayerSettings.allowedAutorotateToLandscapeLeft = _landscapeLeft;
                PlayerSettings.allowedAutorotateToLandscapeRight = _landscapeRight;
                PlayerSettings.usePlayerLog = _playerLog;
                PlayerSettings.colorSpace = _colorSpace;
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, _backend);
                PlayerSettings.Android.targetArchitectures = _architectures;
                PlayerSettings.Android.forceInternetPermission = _internet;
                PlayerSettings.Android.forceSDCardPermission = _sdCard;
                PlayerSettings.Android.splitApplicationBinary = _split;
                PlayerSettings.Android.minifyRelease = _minify;
                PlayerSettings.Android.appCategory = _appCategory;
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, _graphics);
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, _defaultGraphics);
                EditorSettings.defaultBehaviorMode = _behavior;
                EditorSettings.serializationMode = _serialization;
                EditorBuildSettings.scenes = _scenes;
                if (_hasSetupPref) EditorPrefs.SetBool(EdgeRushSetup.PrefKey, _setupPref);
                else EditorPrefs.DeleteKey(EdgeRushSetup.PrefKey);
            }
        }
    }
}
