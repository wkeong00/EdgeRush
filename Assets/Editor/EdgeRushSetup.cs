using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EdgeRush.EditorTools
{
    /// <summary>
    /// One-click gameplay configuration and boot scene setup. Release identity and signing
    /// are maintained separately and remain untouched even when this runs on another machine.
    /// Runs automatically the first time the project is opened, and from the EdgeRush menu.
    /// </summary>
    [InitializeOnLoad]
    public static class EdgeRushSetup
    {
        internal const string ScenePath = "Assets/Scenes/Main.unity";
        internal static string PrefKey => "EdgeRush.SetupDone." + Application.dataPath.GetHashCode();

        static EdgeRushSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        static void AutoSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool(PrefKey, false)) return;
            TryApply(false);
        }

        [MenuItem("EdgeRush/Apply Project Settings", priority = 1)]
        public static void ApplyMenu()
        {
            TryApply(true);
        }

        [MenuItem("EdgeRush/Rebuild Main Scene", priority = 2)]
        public static void RebuildSceneMenu()
        {
            EnsureScene(true);
        }

        static void TryApply(bool verbose)
        {
            try
            {
                Apply(verbose);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[EdgeRush] Setup failed: " + e.Message);
            }
        }

        // The checks use this same path so exceptions cannot be swallowed as a passing check.
        internal static void Apply(bool verbose)
        {
            ConfigurePlayerSettings();
            EnsureScene(verbose);
            EditorPrefs.SetBool(PrefKey, true);
            if (verbose) Debug.Log("[EdgeRush] Gameplay project settings applied; release identity, version and signing preserved.");
        }

        static void ConfigurePlayerSettings()
        {
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
            // Never lower an SDK floor chosen for an installed SDK. The target API is an
            // explicit release decision, including Auto, so this helper never writes it.
            if ((int)PlayerSettings.Android.minSdkVersion < (int)AndroidSdkVersions.AndroidApiLevel26)
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.forceSDCardPermission = false;
            PlayerSettings.Android.splitApplicationBinary = false;
            PlayerSettings.Android.minifyRelease = false;
            PlayerSettings.Android.appCategory = "game";

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
                // Do not replace or silently save the user's Untitled scene. Unity also
                // rejects additive creation in this state, so report the required action.
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    if (string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                        throw new System.InvalidOperationException("Save or close Untitled scenes before creating the EdgeRush boot scene.");
                var active = SceneManager.GetActiveScene();
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                try
                {
                    if (!EditorSceneManager.SaveScene(scene, ScenePath))
                        throw new IOException("Could not save the EdgeRush boot scene.");
                }
                finally
                {
                    if (active.IsValid() && active.isLoaded)
                    {
                        SceneManager.SetActiveScene(active);
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
                if (verbose) Debug.Log("[EdgeRush] Created " + ScenePath);
            }

            bool already = false;
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path == ScenePath) already = true;

            if (!already)
            {
                var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            AssetDatabase.SaveAssets();
        }
    }
}
