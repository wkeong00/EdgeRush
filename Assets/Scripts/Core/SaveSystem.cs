using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace EdgeRush
{
    [Serializable]
    public class QuestState
    {
        public string id;
        public int progress;
        public bool claimed;
    }

    [Serializable]
    public class EconomyReceipt
    {
        public string key;
        public string source;
        public int amount;
        public int balance;
        public string utc;
    }

    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 2;
        public int version = CurrentVersion;
        public long saveRevision;
        public int bestScore;
        public int bestComboLevel = 1;
        public int bestDistance;
        public int totalRuns;
        public int completedRuns;
        public int totalDistance;
        public int totalNearMisses;
        public int totalCoinsEarned;
        public int totalAdViews;
        public int coins;
        public List<string> ownedSkins = new List<string>();
        public string selectedSkin = "cyan";
        public int adSkinProgress;
        public List<EconomyReceipt> economyReceipts = new List<EconomyReceipt>();
        public int tutorialVersion;
        public bool tutorialCompleted;
        public string dailyDate = "";
        public int dailyStreak;
        public string dailyAdDate = "";
        public int dailyAdCount;
        public string questDate = "";
        public List<QuestState> quests = new List<QuestState>();
        public bool musicOn = true;
        public bool sfxOn = true;
        public bool hapticsOn = true;
        public bool reducedEffects;
        public string firstLaunchDate = "";
        public string lastLaunchDate = "";
        public int sessions;

        public bool Owns(string skinId) => ownedSkins != null && ownedSkins.Contains(skinId);

        public void Sanitize()
        {
            if (version < CurrentVersion)
            {
                // v1 recorded starts only. Keep established players' onboarding/ad access.
                completedRuns = Math.Max(completedRuns, totalRuns);
                if (totalRuns > 0) { tutorialCompleted = true; tutorialVersion = 1; }
                version = CurrentVersion;
            }
            if (ownedSkins == null) ownedSkins = new List<string>();
            if (quests == null) quests = new List<QuestState>();
            if (economyReceipts == null) economyReceipts = new List<EconomyReceipt>();
            if (!ownedSkins.Contains("cyan")) ownedSkins.Insert(0, "cyan");
            if (string.IsNullOrEmpty(selectedSkin) || !ownedSkins.Contains(selectedSkin)) selectedSkin = "cyan";
            coins = Math.Max(0, coins);
            bestComboLevel = Math.Max(1, bestComboLevel);
            completedRuns = Math.Max(0, completedRuns);
            dailyStreak = Math.Max(0, Math.Min(7, dailyStreak));
            dailyAdCount = Math.Max(0, dailyAdCount);
            adSkinProgress = Math.Max(0, adSkinProgress);
        }
    }

    /// <summary>Atomic file plus independent backup. XOR/checksum detects corruption, not cheating.</summary>
    public class SaveSystem
    {
        const string PrefBackup = "edgerush.savedata";
        const string FileName = "edgerush.dat";
        static readonly byte[] Key = Encoding.UTF8.GetBytes("EdgeRush::v1::d0ntEditTh1s");
        readonly string _directory;
        readonly string _backupKey;
        bool _preserveBackupOnNextSave;
        public SaveData Data { get; private set; } = new SaveData();
        public bool LoadFailed { get; private set; }
        public bool RecoveredFromBackup { get; private set; }
        public bool LastSaveSucceeded { get; private set; }
        string FilePath => Path.Combine(_directory, FileName);

        // Empty backupKey disables PlayerPrefs, useful for isolated verification sessions.
        public SaveSystem(string storageDirectory = null, string backupKey = null)
        {
            _directory = storageDirectory ?? Application.persistentDataPath;
            _backupKey = backupKey ?? PrefBackup;
        }

        public static string Today => DateTime.Now.ToString("yyyy-MM-dd");
        public static string Yesterday => DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        public static int DaySeed => int.Parse(DateTime.Now.ToString("yyyyMMdd"));

        public void Load()
        {
            SaveData loaded = ReadFile(FilePath);
            RecoveredFromBackup = false;
            // A previous file write may have failed while PlayerPrefs committed a newer reward.
            // Validate every copy and select its revision rather than preferring a stale primary.
            if (!string.IsNullOrEmpty(_backupKey))
            {
                try
                {
                    var prefs = Decode(PlayerPrefs.GetString(_backupKey, ""));
                    if (prefs != null && (loaded == null || prefs.saveRevision > loaded.saveRevision))
                    {
                        loaded = prefs;
                        RecoveredFromBackup = true;
                    }
                }
                catch (Exception e) { Debug.LogWarning("[Save] backup load failed: " + e.Message); }
            }
            var previous = ReadFile(FilePath + ".bak");
            if (previous != null && (loaded == null || previous.saveRevision > loaded.saveRevision))
            {
                loaded = previous;
                RecoveredFromBackup = true;
            }
            LoadFailed = loaded == null && (File.Exists(FilePath) || File.Exists(FilePath + ".bak") ||
                (!string.IsNullOrEmpty(_backupKey) && PlayerPrefs.HasKey(_backupKey)));
            _preserveBackupOnNextSave = RecoveredFromBackup;
            Data = loaded ?? new SaveData();
            Data.Sanitize();
            if (string.IsNullOrEmpty(Data.firstLaunchDate)) Data.firstLaunchDate = Today;
            Data.sessions++;
            Data.lastLaunchDate = Today;
            Save();
        }

        static SaveData ReadFile(string path)
        {
            try { return File.Exists(path) ? Decode(File.ReadAllText(path)) : null; }
            catch (Exception e) { Debug.LogWarning("[Save] file load failed: " + e.Message); return null; }
        }

        public void Save()
        {
            LastSaveSucceeded = false;
            if (Data == null) return;
            // One revision identifies the same commit in every persistence backend.
            if (Data.saveRevision < long.MaxValue) Data.saveRevision = Math.Max(0L, Data.saveRevision) + 1;
            string encoded;
            try { encoded = Encode(Data); }
            catch (Exception e) { Debug.LogWarning("[Save] encoding failed: " + e.Message); return; }
            try
            {
                Directory.CreateDirectory(_directory);
                string staging = FilePath + ".tmp";
                File.WriteAllText(staging, encoded);
                // A corrupt primary must never replace the verified copy used for recovery.
                if (File.Exists(FilePath)) File.Replace(staging, FilePath, _preserveBackupOnNextSave ? null : FilePath + ".bak");
                else File.Move(staging, FilePath);
                _preserveBackupOnNextSave = false;
                LastSaveSucceeded = true;
            }
            catch (Exception e) { Debug.LogWarning("[Save] file write failed: " + e.Message); }
            if (string.IsNullOrEmpty(_backupKey)) return;
            try
            {
                PlayerPrefs.SetString(_backupKey, encoded);
                PlayerPrefs.Save();
                LastSaveSucceeded = true;
            }
            catch (Exception e) { Debug.LogWarning("[Save] backup write failed: " + e.Message); }
        }

        static string Encode(SaveData data)
        {
            var plain = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
            uint hash = Fnv(plain);
            var bytes = new byte[plain.Length + 4];
            bytes[0] = (byte)hash;
            bytes[1] = (byte)(hash >> 8);
            bytes[2] = (byte)(hash >> 16);
            bytes[3] = (byte)(hash >> 24);
            for (int i = 0; i < plain.Length; i++) bytes[i + 4] = (byte)(plain[i] ^ Key[i % Key.Length] ^ (i * 31 & 255));
            return Convert.ToBase64String(bytes);
        }

        static SaveData Decode(string encoded)
        {
            try
            {
                var raw = Convert.FromBase64String(encoded);
                if (raw.Length < 5) return null;
                uint hash = (uint)(raw[0] | raw[1] << 8 | raw[2] << 16 | raw[3] << 24);
                var plain = new byte[raw.Length - 4];
                for (int i = 0; i < plain.Length; i++) plain[i] = (byte)(raw[i + 4] ^ Key[i % Key.Length] ^ (i * 31 & 255));
                if (Fnv(plain) != hash) return null;
                return JsonUtility.FromJson<SaveData>(Encoding.UTF8.GetString(plain));
            }
            catch { return null; }
        }

        static uint Fnv(byte[] data)
        {
            uint hash = 2166136261u;
            for (int i = 0; i < data.Length; i++) { hash ^= data[i]; hash *= 16777619u; }
            return hash;
        }
    }
}
