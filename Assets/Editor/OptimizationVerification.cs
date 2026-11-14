using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EdgeRush.EditorTools
{
    /// <summary>Runs the real boot/game UI in Play Mode and saves reviewable captures. No SDK/device claims.</summary>
    [InitializeOnLoad]
    public static class OptimizationVerification
    {
        const string Running = "EdgeRush.Verification.Running";
        static IEnumerator _steps;
        static double _next, _started;
        static int _lastFrame;
        static int _runtimeErrors;
        static EditorWindow _gameView;
        static int _expectedWidth, _expectedHeight;
        static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../docs/verification-ab"));

        static OptimizationVerification()
        {
            if (SessionState.GetBool(Running, false)) EditorApplication.update += Tick;
            Application.logMessageReceived += (message, stack, type) =>
            {
                if (SessionState.GetBool(Running, false) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                    _runtimeErrors++;
            };
        }

        public static void RunBatch()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-edgeRushVerification") < 0)
                throw new InvalidOperationException("Verification requires -edgeRushVerification to isolate player saves.");
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output, "result.txt"), "RUNNING\n");
            SessionState.SetBool(Running, true);
            SetSize(900, 1600);
            SessionState.SetBool("EdgeRush.Verification.BackgroundOriginal", PlayerSettings.runInBackground);
            Application.runInBackground = true;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        static void Tick()
        {
            try
            {
                EditorApplication.QueuePlayerLoopUpdate();
                if (_gameView != null) { _gameView.Focus(); _gameView.Repaint(); }
                if (!EditorApplication.isPlaying || GameApp.I == null) return;
                if (Application.isBatchMode && GameApp.I.Suspended)
                {
                    GameApp.I.SendMessage("OnApplicationPause", false);
                    GameApp.I.SendMessage("OnApplicationFocus", true);
                }
                if (_started == 0) _started = EditorApplication.timeSinceStartup;
                if (EditorApplication.timeSinceStartup - _started > 180) throw new Exception("Play verification timed out");
                if (EditorApplication.timeSinceStartup < _next || Time.frameCount == _lastFrame) return;
                _lastFrame = Time.frameCount;
                if (_steps == null) _steps = Verify();
                if (!_steps.MoveNext()) { Finish(0, "PASS: Editor Play Mode checks and captures completed."); return; }
                _next = EditorApplication.timeSinceStartup + (_steps.Current is float ? (float)_steps.Current : 0.25f);
            }
            catch (Exception error) { Debug.LogException(error); Finish(1, error.ToString()); }
        }

        static void Finish(int code, string result)
        {
            File.WriteAllText(Path.Combine(Output, "result.txt"), result + "\n");
            SessionState.SetBool(Running, false);
            EditorApplication.update -= Tick;
            bool originalBackground = SessionState.GetBool("EdgeRush.Verification.BackgroundOriginal", false);
            Application.runInBackground = originalBackground;
            PlayerSettings.runInBackground = originalBackground;
            AssetDatabase.SaveAssets();
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(code);
        }

        static IEnumerator Verify()
        {
            var app = GameApp.I;
            // The batch editor is unfocused. Explicitly simulate focus and record progressing frames.
            app.SendMessage("OnApplicationFocus", true);
            yield return 2f;
            int frame = Time.frameCount;
            yield return 0.25f;
            Require(Time.frameCount > frame, "Player loop did not advance");
            GameplayChecks.Run();
            AdRegressionChecks.Run();
            Debug.Log(EconomyChecks.RunAll());

            app.StartTutorial(true);
            yield return 0.2f;
            app.Player.Move(-1);
            yield return 0.4f;
            Require(app.Player.Lane == 0, "Tutorial left change");
            app.Player.Move(1);
            yield return 2f;
            app.Player.Move(-1);
            yield return 4f;
            Require(app.State == GameState.Menu, "Tutorial did not finish after actual close call");
            Require(app.Run == null, "Tutorial created a scored run");

            foreach (MockAdOutcome outcome in Enum.GetValues(typeof(MockAdOutcome)))
            {
                typeof(AdManager).GetField("_lastShownAt", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(app.Ads, -999f);
                app.Ads.LoadAll();
                yield return 1.1f;
                int reward = 0, closed = 0;
                MockAdProvider.NextOutcome = outcome;
                MockAdProvider.DelaySeconds = 0.1f;
                app.Ads.Show(AdPlacement.DailyBonus, earned => closed++, showId => reward++);
                yield return 1.2f;
                Require(!app.Ads.IsShowing && closed == 1, "Ad terminal path " + outcome);
                Require(reward == (outcome == MockAdOutcome.Success ? 1 : 0), "Ad reward path " + outcome);
            }
            MockAdProvider.DelaySeconds = 3f;

            app.Save.completedRuns = 0;
            for (int completed = 0; completed < 3; completed++)
            {
                app.Save.completedRuns = completed;
                app.Ads.BeginRun();
                Require(!app.Ads.RunAdsEligible, "Early run exposed ads");
                app.Save.completedRuns++;
                Require(!app.Ads.RunAdsEligible, "Third results exposed ads");
            }
            app.Ads.BeginRun();
            Require(app.Ads.RunAdsEligible, "Fourth run should be eligible");

            app.StartRun();
            double deadline = EditorApplication.timeSinceStartup + 10;
            while (app.State == GameState.Countdown && EditorApplication.timeSinceStartup < deadline) yield return 0.1f;
            Require(app.State == GameState.Playing, "Formal countdown did not finish");
            app.PauseGame();
            float frozen = app.Run.Elapsed;
            yield return 0.3f;
            Require(app.Run.Elapsed == frozen && app.State == GameState.Paused, "Pause advanced active clock");
            app.ResumeGame();
            deadline = EditorApplication.timeSinceStartup + 10;
            while (app.State == GameState.Countdown && EditorApplication.timeSinceStartup < deadline) yield return 0.1f;
            Require(app.State == GameState.Playing && app.Run.Elapsed > frozen, "Resume did not restore gameplay");
            app.Obstacles.SpawnCoin(app.Player.Lane, app.Player.Y + 0.1f);
            yield return 0.15f;
            Require(app.Run.Coins > 0, "Real coin pickup did not reach run settlement");
            typeof(AdManager).GetField("_lastShownAt", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(app.Ads, -999f);
            app.Obstacles.Spawn(ObstacleKind.Barrier, app.Player.Lane, app.Player.Y);
            deadline = EditorApplication.timeSinceStartup + 10;
            while (app.State != GameState.ReviveOffer && EditorApplication.timeSinceStartup < deadline) yield return 0.1f;
            Require(app.State == GameState.ReviveOffer, "Real collision did not offer revive");
            MockAdProvider.DelaySeconds = 0.1f;
            app.WatchReviveAd();
            deadline = EditorApplication.timeSinceStartup + 10;
            while (app.State != GameState.Playing && EditorApplication.timeSinceStartup < deadline) yield return 0.1f;
            Require(app.State == GameState.Playing && app.Run.Revived, "Rewarded revive did not resume this run");
            Require(Progression.HasReceipt(app.Save, "run:" + app.RunId + ":revive"), "Revive entitlement was not persisted");
            int beforeSettlement = app.Save.coins;
            int baseCoins = app.Run.Coins;
            typeof(GameApp).GetMethod("FinishRun", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(app, null);
            Require(app.State == GameState.Results, "Formal result flow");
            Require(app.Save.coins == beforeSettlement + baseCoins, "Base settlement was not credited once");
            typeof(AdManager).GetField("_lastShownAt", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(app.Ads, -999f);
            app.WatchDoubleCoins();
            yield return 0.3f;
            Require(app.Save.coins == beforeSettlement + baseCoins * 2 && app.DoubleCoinsUsed && app.Ads.IsShowing,
                "Bonus must be persisted at reward, before close");
            yield return 1f;
            Require(!app.Ads.IsShowing, "Rewarded double did not close");
            app.WatchDoubleCoins();
            Require(app.Save.coins == beforeSettlement + baseCoins * 2 && !app.Ads.IsShowing, "Double reward reentry granted again");
            MockAdProvider.DelaySeconds = 3f;
            app.EnterMenu();

            int[] heights = { 1600, 1800, 1950, 2000, 1920 };
            for (int i = 0; i < heights.Length; i++)
            {
                SetSize(i == 4 ? 1080 : 900, heights[i]);
                yield return 0.5f;
                while (Screen.width != _expectedWidth || Screen.height != _expectedHeight) yield return 0.2f;
                var safeFitter = app.UI.Safe.GetComponent<SafeAreaFitter>();
                safeFitter.enabled = i != 4;
                if (i == 4)
                {
                    app.UI.Safe.anchorMin = new Vector2(0, 0.025f);
                    app.UI.Safe.anchorMax = new Vector2(1, 0.95f);
                    app.UI.Safe.offsetMin = app.UI.Safe.offsetMax = Vector2.zero;
                }
                app.EnterMenu();
                yield return 0.3f;
                Capture(i + "-menu");
                yield return 0.3f;
                app.UI.Quests.Show(true);
                yield return 0.3f;
                Capture(i + "-missions");
                yield return 0.3f;
                app.UI.Quests.Hide(true);
                app.UI.Garage.Show(true);
                yield return 0.3f;
                Capture(i + "-garage");
                yield return 0.3f;
                app.UI.Garage.Hide(true);
                app.StartTutorial(true);
                yield return 0.3f;
                Capture(i + "-tutorial-hud");
                yield return 0.3f;
                app.SkipTutorial();
                app.UI.CloseAllMenus();
                app.UI.Hud.Show(true);
                app.UI.Hud.ShowRunUi(true);
                app.UI.Hud.SetScore(1234);
                app.UI.Hud.SetCoins(28);
                app.UI.Hud.SetCombo(5, 0.6f);
                yield return 0.3f;
                Capture(i + "-hud");
                yield return 0.3f;
                app.UI.Revive.Present(30f, 5);
                yield return 0.3f;
                Capture(i + "-revive");
                yield return 0.3f;
                app.UI.Revive.Close();
                app.UI.Hud.Hide(true);
                app.UI.Results.Present(new RunSummary { score = 1234, metres = 760, nearMisses = 12, comboLevel = 5, coins = 28 },
                    2000, false, 28, true);
                app.UI.Results.Show(true);
                yield return 0.3f;
                Capture(i + "-results");
                yield return 0.3f;
            }
            yield return 0.8f;
            Require(Directory.GetFiles(Output, "*.png").Length >= 35, "Missing screenshots");
            Require(_runtimeErrors == 0, "Unity logged runtime errors; inspect Editor log");
        }

        static void Capture(string name)
        {
            Require(Screen.width == _expectedWidth && Screen.height == _expectedHeight, "Capture resolution changed for " + name);
            Canvas.ForceUpdateCanvases();
            ScreenCapture.CaptureScreenshot(Path.Combine(Output, name + ".png"));
            Debug.Log("[Verification] Capture " + name + " " + Screen.width + "x" + Screen.height);
        }
        static void Require(bool passed, string message) { if (!passed) throw new Exception(message); }

        static void SetSize(int width, int height)
        {
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance").GetValue(null);
            var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { 0 });
            var sizeType = assembly.GetType("UnityEditor.GameViewSize");
            var typeEnum = assembly.GetType("UnityEditor.GameViewSizeType");
            var size = Activator.CreateInstance(sizeType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new object[] { Enum.ToObject(typeEnum, 1), width, height, "Verification " + width + "x" + height }, null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
            int index = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null) - 1;
            var gameView = EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));
            gameView.GetType().GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(gameView, index);
            gameView.Show();
            gameView.Focus();
            _gameView = gameView;
            _expectedWidth = width;
            _expectedHeight = height;
        }
    }
}
