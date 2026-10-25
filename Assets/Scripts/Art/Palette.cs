using UnityEngine;

namespace EdgeRush
{
    public struct ThemeDef
    {
        public string id;
        public string label;
        public Color skyTop;
        public Color skyBottom;
        public Color horizon;      // glow band behind the road
        public Color road;
        public Color shoulder;
        public Color laneLine;
        public Color laneLineDim;
        public Color decor;
        public Color decorGlow;
        public int decorStyle;     // 0 = city poles, 1 = tunnel ribs, 2 = desert cacti
        public float starAlpha;
    }

    public static class Palette
    {
        // --- UI ------------------------------------------------------------------
        public static readonly Color Bg = new Color32(7, 9, 20, 255);
        public static readonly Color PanelFill = new Color32(14, 18, 36, 240);
        public static readonly Color PanelFillSoft = new Color32(20, 26, 48, 215);
        public static readonly Color PanelEdge = new Color32(64, 232, 255, 120);
        public static readonly Color Cyan = new Color32(64, 232, 255, 255);
        public static readonly Color Magenta = new Color32(255, 72, 170, 255);
        public static readonly Color Violet = new Color32(150, 110, 255, 255);
        public static readonly Color Gold = new Color32(255, 196, 64, 255);
        public static readonly Color Lime = new Color32(150, 245, 90, 255);
        public static readonly Color Orange = new Color32(255, 138, 60, 255);
        public static readonly Color Danger = new Color32(255, 82, 66, 255);
        public static readonly Color TextMain = new Color32(238, 246, 255, 255);
        public static readonly Color TextDim = new Color32(146, 162, 194, 255);
        public static readonly Color TextDark = new Color32(10, 14, 26, 255);

        // --- themes --------------------------------------------------------------
        public static readonly ThemeDef City = new ThemeDef
        {
            id = "city", label = "NEON CITY",
            skyTop = new Color32(6, 8, 22, 255),
            skyBottom = new Color32(24, 12, 46, 255),
            horizon = new Color32(58, 38, 120, 255),
            road = new Color32(27, 30, 42, 255),
            shoulder = new Color32(13, 14, 24, 255),
            laneLine = new Color32(64, 232, 255, 255),
            laneLineDim = new Color32(38, 120, 160, 255),
            decor = new Color32(16, 18, 40, 255),
            decorGlow = new Color32(255, 72, 170, 255),
            decorStyle = 0,
            starAlpha = 0.65f,
        };

        public static readonly ThemeDef Tunnel = new ThemeDef
        {
            id = "tunnel", label = "TUNNEL",
            skyTop = new Color32(4, 6, 10, 255),
            skyBottom = new Color32(18, 14, 14, 255),
            horizon = new Color32(120, 70, 20, 255),
            road = new Color32(32, 29, 26, 255),
            shoulder = new Color32(15, 14, 13, 255),
            laneLine = new Color32(255, 176, 64, 255),
            laneLineDim = new Color32(150, 96, 30, 255),
            decor = new Color32(14, 13, 14, 255),
            decorGlow = new Color32(255, 176, 64, 255),
            decorStyle = 1,
            starAlpha = 0.0f,
        };

        public static readonly ThemeDef Desert = new ThemeDef
        {
            id = "desert", label = "DESERT DUSK",
            skyTop = new Color32(30, 14, 40, 255),
            skyBottom = new Color32(120, 46, 40, 255),
            horizon = new Color32(240, 120, 60, 255),
            road = new Color32(42, 32, 29, 255),
            shoulder = new Color32(54, 35, 28, 255),
            laneLine = new Color32(255, 210, 120, 255),
            laneLineDim = new Color32(170, 120, 60, 255),
            decor = new Color32(28, 18, 22, 255),
            decorGlow = new Color32(255, 120, 60, 255),
            decorStyle = 2,
            starAlpha = 0.25f,
        };

        public static readonly ThemeDef[] Themes = { City, Tunnel, Desert };

        public static ThemeDef ThemeById(string id)
        {
            for (int i = 0; i < Themes.Length; i++)
                if (Themes[i].id == id) return Themes[i];
            return City;
        }
    }

    public class SkinDef
    {
        public string id;
        public string name;
        public string blurb;
        public int price;
        public bool adUnlock;
        public Color body;
        public Color accent;
        public Color glow;
        public int shape;      // 0 sport, 1 muscle, 2 formula, 3 gt, 4 rally, 5 hyper
    }

    public static class Skins
    {
        public static readonly SkinDef[] All =
        {
            new SkinDef { id = "cyan", name = "CYAN INTERCEPTOR", blurb = "Standard issue. Fast, clean, unmistakable.",
                price = 0, body = new Color32(38, 196, 232, 255), accent = new Color32(120, 244, 255, 255),
                glow = new Color32(64, 232, 255, 255), shape = 0 },

            new SkinDef { id = "viper", name = "MAGENTA VIPER", blurb = "Wide body, twin stripes, zero subtlety.",
                price = 300, body = new Color32(196, 38, 130, 255), accent = new Color32(255, 128, 196, 255),
                glow = new Color32(255, 72, 170, 255), shape = 1 },

            new SkinDef { id = "bolt", name = "LIME BOLT", blurb = "Featherweight rally frame with a big rear wing.",
                price = 600, body = new Color32(126, 214, 62, 255), accent = new Color32(196, 255, 120, 255),
                glow = new Color32(150, 245, 90, 255), shape = 4 },

            new SkinDef { id = "solar", name = "SOLAR FLARE", blurb = "Track-day prototype. Runs hot.",
                price = 1200, body = new Color32(244, 148, 42, 255), accent = new Color32(255, 214, 128, 255),
                glow = new Color32(255, 170, 60, 255), shape = 2 },

            new SkinDef { id = "phantom", name = "VIOLET PHANTOM", blurb = "Matte purple hypercar with a light bar.",
                price = 2000, body = new Color32(108, 76, 214, 255), accent = new Color32(178, 148, 255, 255),
                glow = new Color32(150, 110, 255, 255), shape = 5 },

            new SkinDef { id = "apex", name = "GOLD APEX", blurb = "The garage trophy. Chrome trim, gold flake.",
                price = 3000, body = new Color32(214, 160, 44, 255), accent = new Color32(255, 232, 160, 255),
                glow = new Color32(255, 200, 80, 255), shape = 3 },

            new SkinDef { id = "ghost", name = "CHROME GHOST", blurb = "Limited. Watch 5 reward videos to forge it.",
                price = 0, adUnlock = true, body = new Color32(176, 196, 214, 255),
                accent = new Color32(240, 250, 255, 255), glow = new Color32(200, 226, 255, 255), shape = 5 },
        };

        public static SkinDef ById(string id)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i].id == id) return All[i];
            return All[0];
        }

        public static int IndexOf(string id)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i].id == id) return i;
            return 0;
        }
    }
}
