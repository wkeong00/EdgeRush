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

    /// <summary>Everything the game persists. Kept flat for Unity's JsonUtility.</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;

        // records
        public int bestScore;
        public int bestComboLevel = 1;
        public int bestDistance;
        public int totalRuns;
        public int totalDistance;
        public int totalNearMisses;
        public int totalCoinsEarned;
        public int totalAdViews;

        // economy
        public int coins;
        public List<string> ownedSkins = new List<string>();
        public string selectedSkin = "cyan";
        public int adSkinProgress;

        // dailies
        public string dailyDate = "";
        public int dailyStreak;
        public string dailyAdDate = "";
        public int dailyAdCount;

        // quests
        public string questDate = "";
        public List<QuestState> quests = new List<QuestState>();

        // settings
        public bool musicOn = true;
        public bool sfxOn = true;
        public bool hapticsOn = true;

        // lifecycle
        public string firstLaunchDate = "";
        public string lastLaunchDate = "";
        public int sessions;

        public bool Owns(string skinId) => ownedSkins != null && ownedSkins.Contains(skinId);

        public void Sanitize()
        {
            if (ownedSkins == null) ownedSkins = new List<string>();
            if (quests == null) quests = new List<QuestState>();
            if (string.IsNullOrEmpty(selectedSkin)) selectedSkin = "cyan";
            if (!ownedSkins.Contains("cyan")) ownedSkins.Insert(0, "cyan");
            if (coins < 0) coins = 0;
            if (bestComboLevel < 1) bestComboLevel = 1;
        }
    }

    /// <summary>
    /// Saves to the app's private data folder with a light obfuscation + checksum so casual
    /// save editing is not worth the trouble. A copy also lives in PlayerPrefs as a backup.
    /// </summary>
    public class SaveSystem
    {
        const string PrefBackup = "edgerush.savedata";
        const string FileName = "edgerush.dat";
        static readonly byte[] Key = Encoding.UTF8.GetBytes("EdgeRush::v1::d0ntEditTh1s");

        public SaveData Data { get; private set; } = new SaveData();
        public bool LoadFailed { get; private set; }

        string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static string Today => DateTime.Now.ToString("yyyy-MM-dd");
        public static string Yesterday => DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        /// <summary>A stable per-day seed so daily content is the same all day.</summary>
        public static int DaySeed => int.Parse(DateTime.Now.ToString("yyyyMMdd"));

        public void Load()
        {
            SaveData loaded = null;
            try
            {
                if (File.Exists(FilePath))
                    loaded = Decode(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Save] file load failed: " + e.Message);
            }

            if (loaded == null)
            {
                try
                {
                    var backup = PlayerPrefs.GetString(PrefBackup, "");
                    if (!string.IsNullOrEmpty(backup)) loaded = Decode(backup);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[Save] backup load failed: " + e.Message);
                }
            }

            if (loaded == null)
            {
                LoadFailed = File.Exists(FilePath) || PlayerPrefs.HasKey(PrefBackup);
                loaded = new SaveData();
            }

            Data = loaded;
            Data.Sanitize();

            if (string.IsNullOrEmpty(Data.firstLaunchDate)) Data.firstLaunchDate = Today;
            Data.sessions++;
            Data.lastLaunchDate = Today;
            Save();
        }

        public void Save()
        {
            if (Data == null) return;
            try
            {
                var encoded = Encode(Data);
                File.WriteAllText(FilePath, encoded);
                PlayerPrefs.SetString(PrefBackup, encoded);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Save] write failed: " + e.Message);
            }
        }

        static string Encode(SaveData data)
        {
            var json = JsonUtility.ToJson(data);
            var plain = Encoding.UTF8.GetBytes(json);
            uint hash = Fnv(plain);
            var outBytes = new byte[plain.Length + 4];
            outBytes[0] = (byte)(hash & 0xFF);
            outBytes[1] = (byte)((hash >> 8) & 0xFF);
            outBytes[2] = (byte)((hash >> 16) & 0xFF);
            outBytes[3] = (byte)((hash >> 24) & 0xFF);
            for (int i = 0; i < plain.Length; i++)
                outBytes[i + 4] = (byte)(plain[i] ^ Key[i % Key.Length] ^ (i * 31 & 0xFF));
            return Convert.ToBase64String(outBytes);
        }

        static SaveData Decode(string encoded)
        {
            try
            {
                var raw = Convert.FromBase64String(encoded);
                if (raw.Length < 5) return null;
                uint hash = (uint)(raw[0] | raw[1] << 8 | raw[2] << 16 | raw[3] << 24);
                var plain = new byte[raw.Length - 4];
                for (int i = 0; i < plain.Length; i++)
                    plain[i] = (byte)(raw[i + 4] ^ Key[i % Key.Length] ^ (i * 31 & 0xFF));
                if (Fnv(plain) != hash) return null;
                return JsonUtility.FromJson<SaveData>(Encoding.UTF8.GetString(plain));
            }
            catch
            {
                return null;
            }
        }

        static uint Fnv(byte[] data)
        {
            uint h = 2166136261u;
            for (int i = 0; i < data.Length; i++)
            {
                h ^= data[i];
                h *= 16777619u;
            }
            return h;
        }
    }
}
