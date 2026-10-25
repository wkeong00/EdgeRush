using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EdgeRush.EditorTools
{
    /// <summary>
    /// One-click project configuration: player settings, the (empty) boot scene, build settings.
    /// Runs automatically the first time the project is opened, and from the EdgeRush menu.
    /// </summary>
    [InitializeOnLoad]
    public static class EdgeRushSetup
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        static string PrefKey => "EdgeRush.SetupDone." + Application.dataPath.GetHashCode();

        static EdgeRushSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        static void AutoSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool(PrefKey, false)) return;
            Apply(false);
        }

        [MenuItem("EdgeRush/Apply Project Settings", priority = 1)]
        public static void ApplyMenu()
        {
            Apply(true);
        }

        [MenuItem("EdgeRush/Rebuild Main Scene", priority = 2)]
        public static void RebuildSceneMenu()
        {
            EnsureScene(true);
        }

        static void Apply(bool verbose)
        {
            try
            {
                ConfigurePlayerSettings();
                EnsureScene(verbose);
                EditorPrefs.SetBool(PrefKey, true);
                if (verbose) Debug.Log("[EdgeRush] Project settings applied.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[EdgeRush] Setup failed: " + e.Message);
            }
        }

        static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Your Studio";
            PlayerSettings.productName = BuildInfo.GameName;
            PlayerSettings.bundleVersion = BuildInfo.Version;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BuildInfo.PackageName);

            // portrait only
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.usePlayerLog = true;

            // 2D friendly rendering
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Android platform
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.forceSDCardPermission = false;
            PlayerSettings.Android.splitApplicationBinary = false;
            PlayerSettings.Android.minifyRelease = false;
            PlayerSettings.Android.androidIsGame = true;
            PlayerSettings.Android.useCustomKeystore = false;

            try
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                    new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[EdgeRush] Could not force GLES3, using defaults: " + e.Message);
            }

            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            AssetDatabase.SaveAssets();
        }

        /// <summary>Creates the empty boot scene (the game builds itself from code) and registers it.</summary>
        static void EnsureScene(bool verbose)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
                if (verbose) Debug.Log("[EdgeRush] Created " + ScenePath);
            }

            bool already = false;
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path == ScenePath) already = true;

            if (!already)
            {
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(ScenePath, true),
                };
            }

            AssetDatabase.SaveAssets();
        }
    }
}
