using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeRush
{
    public struct RunSummary
    {
        public int score;
        public int metres;
        public int nearMisses;
        public int comboLevel;
        public int coins;
        public bool revived;
        public float seconds;
        public float worldSeconds;
        public float realSeconds;
        public string runId;
    }

    public enum QuestMetric
    {
        Runs,
        Coins,
        NearMissBest,
        ScoreBest,
        DistanceBest,
        ComboBest,
    }

    public class QuestDef
    {
        public string id;
        public string title;
        public string hint;
        public int target;
        public int reward;
        public QuestMetric metric;
    }

    /// <summary>Daily rewards, daily quests, skins and coin accounting.</summary>
    public static class Progression
    {
        public static readonly int[] DailyRewards = { 50, 75, 100, 150, 200, 300, 500 };

        static readonly QuestDef[] QuestPool =
        {
            new QuestDef { id = "q_runs5", title = "PLAY 5 RUNS", hint = "Any mode, any score.", target = 5, reward = 60, metric = QuestMetric.Runs },
            new QuestDef { id = "q_coins40", title = "COLLECT 40 COINS", hint = "Coins add up across runs today.", target = 40, reward = 70, metric = QuestMetric.Coins },
            new QuestDef { id = "q_near8", title = "8 CLOSE CALLS IN ONE RUN", hint = "Squeeze past traffic at the last moment.", target = 8, reward = 90, metric = QuestMetric.NearMissBest },
            new QuestDef { id = "q_near15", title = "15 CLOSE CALLS IN ONE RUN", hint = "For the brave.", target = 15, reward = 120, metric = QuestMetric.NearMissBest },
            new QuestDef { id = "q_score1500", title = "SCORE 1,500 IN ONE RUN", hint = "Distance, coins and close calls all count.", target = 1500, reward = 80, metric = QuestMetric.ScoreBest },
            new QuestDef { id = "q_score3000", title = "SCORE 3,000 IN ONE RUN", hint = "Deep run required.", target = 3000, reward = 130, metric = QuestMetric.ScoreBest },
            new QuestDef { id = "q_dist800", title = "DRIVE 800 m IN ONE RUN", hint = "Survive past the two minute mark.", target = 800, reward = 90, metric = QuestMetric.DistanceBest },
            new QuestDef { id = "q_combo4", title = "REACH A x4 MULTIPLIER", hint = "Nine close calls without a five second gap.", target = 4, reward = 100, metric = QuestMetric.ComboBest },
        };

        public static QuestDef QuestById(string id)
        {
            for (int i = 0; i < QuestPool.Length; i++)
                if (QuestPool[i].id == id) return QuestPool[i];
            return QuestPool[0];
        }

        // Receipts are persisted with the balance; repeating a callback never grants twice.
        public static bool HasReceipt(SaveData s, string key)
        {
            if (s == null || string.IsNullOrEmpty(key) || s.economyReceipts == null) return false;
            for (int i = 0; i < s.economyReceipts.Count; i++)
                if (s.economyReceipts[i] != null && s.economyReceipts[i].key == key) return true;
            return false;
        }

        static void Record(SaveData s, string key, string source, int amount)
        {
            if (s.economyReceipts == null) s.economyReceipts = new List<EconomyReceipt>();
            s.economyReceipts.Add(new EconomyReceipt
            {
                key = key, source = source, amount = amount, balance = s.coins,
                utc = DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
            });
            Analytics.Event("economy_transaction", "source=" + source + " amount=" + amount + " balance=" + s.coins + " key=" + key);
        }

        public static bool TryRecordReward(SaveData s, string key, string source)
        {
            if (s == null || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(source) || HasReceipt(s, key)) return false;
            Record(s, key, source, 0);
            return true;
        }

        public static bool GrantCoins(SaveData s, int amount, string source, string key)
        {
            if (s == null || amount <= 0 || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(source) || HasReceipt(s, key)) return false;
            if ((long)s.coins + amount > int.MaxValue || (long)s.totalCoinsEarned + amount > int.MaxValue) return false;
            s.coins += amount;
            s.totalCoinsEarned += amount;
            Record(s, key, source, amount);
            return true;
        }

        // Compatibility for callers without a semantic key; new rewards should supply one.
        public static void AddCoins(SaveData s, int amount) => GrantCoins(s, amount, "legacy", Guid.NewGuid().ToString("N"));

        public static bool TryBuySkin(SaveData s, SkinDef skin)
        {
            if (skin == null || skin.adUnlock) return false;
            if (s.Owns(skin.id)) return true;
            if (skin.price < 0 || s.coins < skin.price) return false;
            s.coins -= skin.price;
            s.ownedSkins.Add(skin.id);
            s.selectedSkin = skin.id;
            Record(s, "skin:" + skin.id, "skin_purchase", -skin.price);
            Analytics.SkinUnlocked(skin.id);
            return true;
        }

        public static bool SelectSkin(SaveData s, SkinDef skin)
        {
            if (skin == null || !s.Owns(skin.id)) return false;
            s.selectedSkin = skin.id;
            return true;
        }

        /// <summary>Returns true only when this view unlocks the skin, preserving existing UI semantics.</summary>
        public static bool AddAdSkinProgress(SaveData s, string key = null)
        {
            if (s.Owns("ghost")) return false;
            key = key ?? ("skin_view:" + Guid.NewGuid().ToString("N"));
            if (!TryRecordReward(s, key, "ad_skin")) return false;
            s.adSkinProgress++;
            if (s.adSkinProgress < Tuning.SkinAdCount) return false;
            var ghost = Skins.ById("ghost");
            s.ownedSkins.Add(ghost.id);
            s.selectedSkin = ghost.id;
            Analytics.SkinUnlocked(ghost.id);
            return true;
        }

        public static bool CanClaimDaily(SaveData s) => s.dailyDate != SaveSystem.Today;

        /// <summary>Progress survives missed days; after day seven the next claim starts a new cycle.</summary>
        public static int NextDailyDay(SaveData s)
        {
            if (s.dailyDate == SaveSystem.Today) return Mathf.Clamp(s.dailyStreak, 1, 7);
            return s.dailyStreak >= 7 ? 1 : Mathf.Clamp(s.dailyStreak + 1, 1, 7);
        }

        public static int DailyRewardForDay(int day) => DailyRewards[Mathf.Clamp(day - 1, 0, DailyRewards.Length - 1)];

        public static int ClaimDaily(SaveData s) => ClaimDailyOn(s, SaveSystem.Today);

        // Explicit day keeps the calendar boundary testable without changing the device clock.
        public static int ClaimDailyOn(SaveData s, string today)
        {
            if (s.dailyDate == today) return 0;
            int day = s.dailyStreak >= 7 ? 1 : Mathf.Clamp(s.dailyStreak + 1, 1, 7);
            int coins = DailyRewardForDay(day);
            if (!GrantCoins(s, coins, "daily_gift", "daily:" + today)) return 0;
            s.dailyStreak = day;
            s.dailyDate = today;
            Analytics.DailyClaimed(day, coins);
            return coins;
        }

        public static int DailyAdsLeft(SaveData s)
        {
            if (s.dailyAdDate != SaveSystem.Today) return Tuning.DailyAdMax;
            return Mathf.Max(0, Tuning.DailyAdMax - s.dailyAdCount);
        }

        public static int ClaimDailyAd(SaveData s, string key = null)
        {
            if (s.dailyAdDate != SaveSystem.Today)
            {
                s.dailyAdDate = SaveSystem.Today;
                s.dailyAdCount = 0;
            }
            if (s.dailyAdCount >= Tuning.DailyAdMax) return 0;
            key = key ?? ("daily_ad:" + Guid.NewGuid().ToString("N"));
            if (!GrantCoins(s, Tuning.DailyAdCoins, "ad_daily", key)) return 0;
            s.dailyAdCount++;
            return Tuning.DailyAdCoins;
        }

        public static void EnsureQuests(SaveData s)
        {
            if (s.questDate == SaveSystem.Today && s.quests != null && s.quests.Count == 3) return;
            s.questDate = SaveSystem.Today;
            s.quests = new List<QuestState>();
            var rnd = new System.Random(SaveSystem.DaySeed);
            // Keep today's existing set. New days include easy, close-call practice and advanced goals.
            string[][] groups =
            {
                new[] { "q_runs5", "q_coins40" },
                new[] { "q_near8" },
                new[] { "q_near15", "q_score1500", "q_score3000", "q_dist800", "q_combo4" },
            };
            for (int i = 0; i < groups.Length; i++)
                s.quests.Add(new QuestState { id = groups[i][rnd.Next(groups[i].Length)] });
        }

        public static bool ReportRun(SaveData s, RunSummary run, string runId = null)
        {
            string key = "run:" + (runId ?? run.runId ?? Guid.NewGuid().ToString("N")) + ":completed";
            if (!TryRecordReward(s, key, "run_completed")) return false;
            s.completedRuns++;
            EnsureQuests(s);
            foreach (var q in s.quests)
            {
                if (q == null) continue;
                var def = QuestById(q.id);
                switch (def.metric)
                {
                    case QuestMetric.Runs: q.progress += 1; break;
                    case QuestMetric.Coins: q.progress += run.coins; break;
                    case QuestMetric.NearMissBest: q.progress = Mathf.Max(q.progress, run.nearMisses); break;
                    case QuestMetric.ScoreBest: q.progress = Mathf.Max(q.progress, run.score); break;
                    case QuestMetric.DistanceBest: q.progress = Mathf.Max(q.progress, run.metres); break;
                    case QuestMetric.ComboBest: q.progress = Mathf.Max(q.progress, run.comboLevel); break;
                }
            }
            return true;
        }

        public static bool CanClaimQuest(QuestState q, out QuestDef def)
        {
            def = q == null ? null : QuestById(q.id);
            return q != null && !q.claimed && def.id == q.id && q.progress >= def.target;
        }

        public static int ClaimQuest(SaveData s, QuestState q)
        {
            QuestDef def;
            if (s.questDate != SaveSystem.Today || s.quests == null || !s.quests.Contains(q) || !CanClaimQuest(q, out def)) return 0;
            if (!GrantCoins(s, def.reward, "quest", "quest:" + s.questDate + ":" + q.id)) return 0;
            q.claimed = true;
            Analytics.QuestClaimed(q.id, def.reward);
            return def.reward;
        }
    }
}
