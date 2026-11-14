using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EdgeRush.EditorTools
{
    /// <summary>Deterministic regression checks; all file operations use an isolated Temp folder.</summary>
    public static class EconomyChecks
    {
        static int _checks;

        [MenuItem("Edge Rush/Verify Economy And Saves")]
        public static void RunFromMenu() => Debug.Log(RunAll());

        public static string RunAll()
        {
            _checks = 0;
            CheckRewards();
            CheckDailyCycle();
            CheckAdsAndQuests();
            CheckMigration();
            CheckSaveRecovery();
            CheckNewestCopySelection();
            return "Economy/save checks passed: " + _checks;
        }

        static void Require(bool condition, string message)
        {
            _checks++;
            if (!condition) throw new InvalidOperationException("Economy regression: " + message);
        }

        static void CheckRewards()
        {
            var s = new SaveData();
            s.Sanitize();
            Require(Progression.GrantCoins(s, 12, "run_settlement", "run:a:settlement"), "settlement granted");
            Require(!Progression.GrantCoins(s, 12, "run_settlement", "run:a:settlement"), "settlement duplicate rejected");
            Require(Progression.GrantCoins(s, 12, "ad_double", "run:a:double"), "one extra settlement granted");
            Require(!Progression.GrantCoins(s, 500, "ad_double", "run:a:double"), "duplicate cannot change amount");
            Require(s.coins == 24 && s.totalCoinsEarned == 24, "balance includes base and bonus exactly once");
            Require(Progression.TryRecordReward(s, "run:a:revive", "ad_revive"), "non-currency entitlement granted");
            Require(!Progression.TryRecordReward(s, "run:a:revive", "ad_revive"), "revive duplicate rejected");
            Require(s.economyReceipts.Count == 3 && s.economyReceipts[1].balance == 24, "receipts capture balances");
            Require(!Progression.GrantCoins(s, -1, "bad", "bad"), "negative grant rejected");
            var restored = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(s));
            restored.Sanitize();
            Require(!Progression.GrantCoins(restored, 12, "ad_double", "run:a:double"), "idempotency survives serialization");
        }

        static void CheckDailyCycle()
        {
            var s = new SaveData();
            var date = new DateTime(2026, 1, 1);
            int earned = 0;
            for (int i = 0; i < 8; i++)
            {
                string day = date.AddDays(i).ToString("yyyy-MM-dd");
                int expected = Progression.DailyRewards[i % 7];
                Require(Progression.ClaimDailyOn(s, day) == expected, "daily reward at claim " + (i + 1));
                Require(Progression.ClaimDailyOn(s, day) == 0, "same-day duplicate at claim " + (i + 1));
                earned += expected;
            }
            Require(s.dailyStreak == 1 && s.coins == earned, "day eight restarts cycle");
            Require(Progression.ClaimDailyOn(s, "2026-03-10") == Progression.DailyRewards[1], "missed days keep cycle progress");
            Require(s.dailyStreak == 2, "missed day advances once");
        }

        static void CheckAdsAndQuests()
        {
            var s = new SaveData();
            s.Sanitize();
            for (int i = 0; i < Tuning.DailyAdMax; i++)
            {
                Require(Progression.ClaimDailyAd(s, "daily-view:" + i) == Tuning.DailyAdCoins, "daily ad unique reward");
                Require(Progression.ClaimDailyAd(s, "daily-view:" + i) == 0, "daily ad callback duplicate");
            }
            Require(Progression.ClaimDailyAd(s, "daily-view:overflow") == 0 && Progression.DailyAdsLeft(s) == 0, "daily ad cap");
            Progression.AddAdSkinProgress(s, "ghost:1");
            Progression.AddAdSkinProgress(s, "ghost:1");
            Require(s.adSkinProgress == 1, "skin view duplicate rejected");
            for (int i = 2; i <= Tuning.SkinAdCount; i++) Progression.AddAdSkinProgress(s, "ghost:" + i);
            Require(s.Owns("ghost") && s.adSkinProgress == Tuning.SkinAdCount, "skin unlock at exact threshold");
            Progression.EnsureQuests(s);
            var todayQuests = s.quests;
            Progression.EnsureQuests(s);
            Require(ReferenceEquals(todayQuests, s.quests), "today's quest progress preserved");
            var run = new RunSummary { coins = 5, nearMisses = 2, score = 100, metres = 50, comboLevel = 2 };
            Require(Progression.ReportRun(s, run, "run-one"), "run completion accepted");
            string before = JsonUtility.ToJson(s);
            Require(!Progression.ReportRun(s, run, "run-one") && s.completedRuns == 1 && before == JsonUtility.ToJson(s), "run completion duplicate cannot change counters");
            var q = s.quests[0];
            q.progress = Progression.QuestById(q.id).target;
            int reward = Progression.ClaimQuest(s, q);
            Require(reward > 0 && Progression.ClaimQuest(s, q) == 0, "quest reward once");
            var stale = new QuestState { id = q.id, progress = q.progress };
            Require(Progression.ClaimQuest(s, stale) == 0, "stale page quest rejected");
        }

        static void CheckMigration()
        {
            var s = JsonUtility.FromJson<SaveData>("{\"version\":1,\"coins\":987,\"totalRuns\":9,\"ownedSkins\":[\"cyan\",\"ghost\"],\"selectedSkin\":\"ghost\",\"adSkinProgress\":5,\"dailyStreak\":7}");
            s.Sanitize();
            Require(s.version == SaveData.CurrentVersion && s.coins == 987, "v1 upgrades without losing balance");
            Require(s.Owns("ghost") && s.selectedSkin == "ghost" && s.adSkinProgress == 5, "v1 ownership/progress preserved");
            Require(s.completedRuns == 9 && s.tutorialCompleted && s.tutorialVersion == 1, "existing players retain onboarding eligibility");
            Require(s.economyReceipts != null && !s.reducedEffects, "new fields have safe defaults");
            var fresh = new SaveData { ownedSkins = null, quests = null, economyReceipts = null, coins = -4, bestComboLevel = 0 };
            fresh.Sanitize();
            Require(fresh.Owns("cyan") && fresh.coins == 0 && fresh.bestComboLevel == 1, "invalid/null fields repaired");
            Require(!fresh.tutorialCompleted && fresh.completedRuns == 0, "new player still gets tutorial");
        }

        static void CheckNewestCopySelection()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "EconomyChecks"));
            string directory = Path.Combine(root, Guid.NewGuid().ToString("N"));
            string testBackupKey = "edgerush.economychecks." + Guid.NewGuid().ToString("N");
            try
            {
                var saver = new SaveSystem(directory, testBackupKey);
                saver.Load();
                saver.Data.coins = 10;
                saver.Save();
                string primaryPath = Path.Combine(directory, "edgerush.dat");
                string olderPrimary = File.ReadAllText(primaryPath);
                long olderRevision = saver.Data.saveRevision;
                Require(Progression.GrantCoins(saver.Data, 20, "ad_double", "new-prefs-reward"), "new backup reward granted");
                saver.Save();
                long newerRevision = saver.Data.saveRevision;
                Require(newerRevision > olderRevision, "save revisions increase");
                // Simulate a failed primary replacement with an independently successful Prefs commit.
                File.WriteAllText(primaryPath, olderPrimary);
                var fromPrefs = new SaveSystem(directory, testBackupKey);
                fromPrefs.Load();
                Require(fromPrefs.RecoveredFromBackup && fromPrefs.Data.coins == 30, "newer valid Prefs wins over stale valid primary");
                Require(Progression.HasReceipt(fromPrefs.Data, "new-prefs-reward") &&
                    !Progression.GrantCoins(fromPrefs.Data, 20, "ad_double", "new-prefs-reward"), "newest backup retains reward receipt");
                Require(fromPrefs.Data.saveRevision > newerRevision, "recovered snapshot commits a newer shared revision");
                string newestEncoded = File.ReadAllText(primaryPath);
                File.WriteAllText(primaryPath, olderPrimary);
                File.WriteAllText(primaryPath + ".bak", newestEncoded);
                var fromPrevious = new SaveSystem(directory, "");
                fromPrevious.Load();
                Require(fromPrevious.RecoveredFromBackup && fromPrevious.Data.coins == 30 &&
                    Progression.HasReceipt(fromPrevious.Data, "new-prefs-reward"), "newest previous file wins over stale primary");
            }
            finally
            {
                // This random key belongs only to this check; never touch the user's real backup.
                PlayerPrefs.DeleteKey(testBackupKey);
                PlayerPrefs.Save();
            }
        }
        static void CheckSaveRecovery()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "EconomyChecks"));
            string directory = Path.Combine(root, Guid.NewGuid().ToString("N"));
            var saver = new SaveSystem(directory, "");
            saver.Load();
            saver.Data.coins = 10;
            saver.Save();
            saver.Data.coins = 30;
            saver.Save();
            Require(saver.LastSaveSucceeded && File.Exists(Path.Combine(directory, "edgerush.dat.bak")), "atomic replacement creates prior generation");
            File.WriteAllText(Path.Combine(directory, "edgerush.dat"), "corrupt");
            var recovered = new SaveSystem(directory, "");
            recovered.Load();
            Require(recovered.RecoveredFromBackup && !recovered.LoadFailed && recovered.Data.coins == 10, "bad primary falls back to verified previous generation");
            File.WriteAllText(Path.Combine(directory, "edgerush.dat"), "corrupt");
            var recoveredAgain = new SaveSystem(directory, "");
            recoveredAgain.Load();
            Require(recoveredAgain.RecoveredFromBackup && recoveredAgain.Data.coins == 10, "second corrupt primary still recovers from preserved verified backup");
            File.WriteAllText(Path.Combine(directory, "edgerush.dat"), "corrupt");
            File.WriteAllText(Path.Combine(directory, "edgerush.dat.bak"), "also-corrupt");
            var reset = new SaveSystem(directory, "");
            reset.Load();
            Require(reset.LoadFailed && reset.Data.coins == 0 && reset.Data.Owns("cyan"), "all corrupt copies recover to sanitized new save");
        }
    }
}
