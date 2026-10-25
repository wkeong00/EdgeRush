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

        // ------------------------------------------------------------- economy
        public static void AddCoins(SaveData s, int amount)
        {
            if (amount <= 0) return;
            s.coins += amount;
            s.totalCoinsEarned += amount;
        }

        public static bool TryBuySkin(SaveData s, SkinDef skin)
        {
            if (skin == null || skin.adUnlock) return false;
            if (s.Owns(skin.id)) return true;
            if (skin.price > 0 && s.coins < skin.price) return false;
            s.coins -= skin.price;
            s.ownedSkins.Add(skin.id);
            s.selectedSkin = skin.id;
            Analytics.SkinUnlocked(skin.id);
            return true;
        }

        public static bool SelectSkin(SaveData s, SkinDef skin)
        {
            if (skin == null || !s.Owns(skin.id)) return false;
            s.selectedSkin = skin.id;
            return true;
        }

        /// <summary>Registers one rewarded video watched towards the Chrome Ghost skin.</summary>
        public static bool AddAdSkinProgress(SaveData s)
        {
            var ghost = Skins.ById("ghost");
            if (s.Owns("ghost")) return false;
            s.adSkinProgress++;
            if (s.adSkinProgress >= Tuning.SkinAdCount)
            {
                s.ownedSkins.Add(ghost.id);
                s.selectedSkin = ghost.id;
                Analytics.SkinUnlocked(ghost.id);
                return true;
            }
            return false;
        }

        // --------------------------------------------------------- daily gift
        public static bool CanClaimDaily(SaveData s) => s.dailyDate != SaveSystem.Today;

        /// <summary>1-based day of the 7 day cycle that would be claimed next.</summary>
        public static int NextDailyDay(SaveData s)
        {
            if (s.dailyDate == SaveSystem.Yesterday) return Mathf.Clamp(s.dailyStreak + 1, 1, 7);
            return 1;
        }

        public static int DailyRewardForDay(int day) => DailyRewards[Mathf.Clamp(day - 1, 0, DailyRewards.Length - 1)];

        public static int ClaimDaily(SaveData s)
        {
            if (!CanClaimDaily(s)) return 0;
            if (s.dailyDate == SaveSystem.Yesterday) s.dailyStreak = Mathf.Clamp(s.dailyStreak + 1, 1, 7);
            else s.dailyStreak = 1;
            s.dailyDate = SaveSystem.Today;
            int coins = DailyRewardForDay(s.dailyStreak);
            AddCoins(s, coins);
            Analytics.DailyClaimed(s.dailyStreak, coins);
            return coins;
        }

        public static int DailyAdsLeft(SaveData s)
        {
            if (s.dailyAdDate != SaveSystem.Today) return Tuning.DailyAdMax;
            return Mathf.Max(0, Tuning.DailyAdMax - s.dailyAdCount);
        }

        public static int ClaimDailyAd(SaveData s)
        {
            if (s.dailyAdDate != SaveSystem.Today)
            {
                s.dailyAdDate = SaveSystem.Today;
                s.dailyAdCount = 0;
            }
            if (s.dailyAdCount >= Tuning.DailyAdMax) return 0;
            s.dailyAdCount++;
            AddCoins(s, Tuning.DailyAdCoins);
            return Tuning.DailyAdCoins;
        }

        // ------------------------------------------------------------- quests
        public static void EnsureQuests(SaveData s)
        {
            if (s.questDate == SaveSystem.Today && s.quests != null && s.quests.Count == 3) return;

            s.questDate = SaveSystem.Today;
            s.quests = new List<QuestState>();
            var rnd = new System.Random(SaveSystem.DaySeed);
            var indices = new List<int>();
            for (int i = 0; i < QuestPool.Length; i++) indices.Add(i);
            // Fisher-Yates, then take three that do not repeat a metric
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                int tmp = indices[i]; indices[i] = indices[j]; indices[j] = tmp;
            }
            var usedMetrics = new List<QuestMetric>();
            foreach (var idx in indices)
            {
                var def = QuestPool[idx];
                if (usedMetrics.Contains(def.metric)) continue;
                usedMetrics.Add(def.metric);
                s.quests.Add(new QuestState { id = def.id, progress = 0, claimed = false });
                if (s.quests.Count == 3) break;
            }
        }

        public static void ReportRun(SaveData s, RunSummary run)
        {
            EnsureQuests(s);
            foreach (var q in s.quests)
            {
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
        }

        public static bool CanClaimQuest(QuestState q, out QuestDef def)
        {
            def = QuestById(q.id);
            return !q.claimed && q.progress >= def.target;
        }

        public static int ClaimQuest(SaveData s, QuestState q)
        {
            QuestDef def;
            if (!CanClaimQuest(q, out def)) return 0;
            q.claimed = true;
            AddCoins(s, def.reward);
            Analytics.QuestClaimed(q.id, def.reward);
            return def.reward;
        }
    }
}
