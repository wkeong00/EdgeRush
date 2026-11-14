using System.Collections.Generic;

namespace EdgeRush
{
    /// <summary>Stable text keys with an English fallback; additional locales override individual keys.</summary>
    public static class Strings
    {
        static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "menu.play", "PLAY" },
            { "menu.tagline", "DODGE CLOSE. CHAIN YOUR WAY TO x5." },
            { "controls.hint", "Tap left / right or swipe to change lanes" },
            { "settings.replay_tutorial", "REPLAY TUTORIAL" },
            { "settings.reduced_effects", "REDUCED EFFECTS" },
            { "tutorial.skip", "SKIP" },
            { "tutorial.left.title", "MOVE LEFT" },
            { "tutorial.left.hint", "Tap the left half or swipe left." },
            { "tutorial.right.title", "MOVE RIGHT" },
            { "tutorial.right.hint", "Tap the right half or swipe right." },
            { "tutorial.near_miss.title", "MAKE A CLOSE CALL" },
            { "tutorial.near_miss.hint", "Wait for traffic to get close, then change lanes." },
            { "tutorial.complete.title", "READY TO RUSH" },
            { "tutorial.complete.hint", "Keep close calls going to reach x5." },
            { "results.again", "RUN AGAIN" },
            { "results.ad_unavailable", "AD UNAVAILABLE" },
        };
        static readonly Dictionary<string, string> Overrides = new Dictionary<string, string>();
        public static string Get(string key)
        {
            string value;
            if (Overrides.TryGetValue(key, out value) || English.TryGetValue(key, out value)) return value;
            return key;
        }
        public static void SetLocale(IDictionary<string, string> values)
        {
            Overrides.Clear();
            if (values == null) return;
            foreach (var pair in values) Overrides[pair.Key] = pair.Value;
        }
    }
}
