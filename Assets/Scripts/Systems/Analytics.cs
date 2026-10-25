using System.Collections.Generic;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>
    /// Minimal analytics facade. Every call is logged and counted locally so the numbers in
    /// section 11.2 of the design doc can be checked during testing. When a real backend
    /// (Firebase, TapDB, Unity Analytics) is added, only this file needs to change.
    /// </summary>
    public static class Analytics
    {
        public class Entry
        {
            public string name;
            public string data;
            public float time;
        }

        public static readonly List<Entry> Events = new List<Entry>();
        static readonly Dictionary<string, int> Counters = new Dictionary<string, int>();
        public const int MaxKept = 400;

        public static void Event(string name, string data = null)
        {
            var e = new Entry { name = name, data = data, time = Time.realtimeSinceStartup };
            Events.Add(e);
            if (Events.Count > MaxKept) Events.RemoveAt(0);

            int c;
            Counters.TryGetValue(name, out c);
            Counters[name] = c + 1;

            if (!string.IsNullOrEmpty(data)) Debug.Log("[AN] " + name + " " + data);
            else Debug.Log("[AN] " + name);
        }

        public static int Count(string name)
        {
            int c;
            return Counters.TryGetValue(name, out c) ? c : 0;
        }

        public static void RunStart(int runIndex)
        {
            Event("run_start", "n=" + runIndex);
        }

        public static void RunEnd(int score, int metres, float seconds, int nearMisses, int comboLevel, int coins, bool revived)
        {
            Event("run_end", string.Format("score={0} m={1} s={2:0.0} near={3} combo=x{4} coins={5} rev={6}",
                score, metres, seconds, nearMisses, comboLevel, coins, revived ? 1 : 0));
            if (revived) Event("revive_used");
        }

        public static void AdImpression(string placement)
        {
            Event("ad_shown", placement);
        }

        public static void AdComplete(string placement)
        {
            Event("ad_complete", placement);
        }

        public static void AdFailed(string placement, string reason)
        {
            Event("ad_failed", placement + ":" + reason);
        }

        public static void AdNotReady(string placement)
        {
            Event("ad_not_ready", placement);
        }

        public static void SkinUnlocked(string id)
        {
            Event("skin_unlock", id);
        }

        public static void DailyClaimed(int day, int coins)
        {
            Event("daily_claim", "day=" + day + " coins=" + coins);
        }

        public static void QuestClaimed(string id, int coins)
        {
            Event("quest_claim", id + ":" + coins);
        }

        public static string Summary()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var kv in Counters) sb.Append(kv.Key).Append('=').Append(kv.Value).Append(' ');
            return sb.ToString();
        }
    }

    /// <summary>Short vibration pulses on Android; silently ignored elsewhere.</summary>
    public static class Haptics
    {
        static bool _enabled = true;
        public static void SetEnabled(bool on) => _enabled = on;

        public static void Light() => Buzz(12);
        public static void Medium() => Buzz(26);
        public static void Strong() => Buzz(55);

        public static void Buzz(int ms)
        {
            if (!_enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    if (vibrator != null) vibrator.Call("vibrate", (long)ms);
                }
                return;
            }
            catch
            {
                // fall through to the generic API
            }
#endif
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
