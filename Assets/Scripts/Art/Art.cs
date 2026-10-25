using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>
    /// Every visual in Edge Rush is generated procedurally at runtime, so the build contains
    /// no external art assets (no licensing risk, tiny download).
    /// </summary>
    public static class Art
    {
        public const float PPU = 90f;      // pixels per world unit for world sprites
        public const float UI_PPU = 100f;  // pixels per canvas unit for UI sprites

        static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Texture2D> TexCache = new Dictionary<string, Texture2D>();

        public static void Reset()
        {
            SpriteCache.Clear();
            TexCache.Clear();
        }

        static Sprite Get(string key, Func<Sprite> build)
        {
            Sprite s;
            if (SpriteCache.TryGetValue(key, out s) && s != null) return s;
            s = build();
            s.name = key;
            SpriteCache[key] = s;
            return s;
        }

        static Texture2D GetTex(string key, Func<Texture2D> build)
        {
            Texture2D t;
            if (TexCache.TryGetValue(key, out t) && t != null) return t;
            t = build();
            t.name = key;
            TexCache[key] = t;
            return t;
        }

        static Sprite Make(int w, int h, Action<TexCanvas> draw, float ppu = PPU, Vector4 border = default(Vector4))
        {
            var canvas = new TexCanvas(w, h);
            draw(canvas);
            var tex = canvas.ToTexture();
            var rect = new Rect(0, 0, w, h);
            var pivot = new Vector2(0.5f, 0.5f);
            if (border.sqrMagnitude > 0f)
                return Sprite.Create(tex, rect, pivot, ppu, 0, SpriteMeshType.FullRect, border);
            return Sprite.Create(tex, rect, pivot, ppu);
        }

        static int Px(float world) => Mathf.RoundToInt(world * PPU);

        /// <summary>Wraps a raw texture (backgrounds, vignette) in a simple sprite.</summary>
        public static Sprite FromTexture(Texture2D tex, float ppu = 100f)
            => Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);

        // ================================================================== cars
        static Sprite MakeCar(Color body, Color accent, Color glow, int shape, bool player, bool mover = false)
        {
            int w = Px(Layout.CarW), h = Px(Layout.CarH);

            return Make(w, h, t =>
            {
                float cx = w * 0.5f, cy = h * 0.5f;
                float hw = w * 0.40f, hh = h * 0.455f;

                // ---- silhouette: main body with a tapered nose and tail ----------
                Func<float, float, float> bodySdf = (x, y) => Mathf.Min(
                    Mathf.Min(
                        TexCanvas.RoundBox(x, y, cx, cy, hw * 0.94f, hh * 0.80f, hw * 0.30f),
                        TexCanvas.RoundBox(x, y, cx, cy + hh * 0.72f, hw * 0.74f, hh * 0.22f, hw * 0.24f)),
                    TexCanvas.RoundBox(x, y, cx, cy - hh * 0.74f, hw * 0.84f, hh * 0.20f, hw * 0.26f));

                // grounding shadow
                t.Paint((x, y) => bodySdf(x, y) - 3f, new Color(0f, 0f, 0f, 0.55f), 1f, 9f);

                // body with a gradient so the nose catches the light
                t.PaintMasked(bodySdf, (x, y) =>
                {
                    float v = Mathf.InverseLerp(cy - hh, cy + hh, y);
                    float k = Mathf.Lerp(0.76f, 1.12f, Mathf.Clamp01(v));
                    return new Color(body.r * k, body.g * k, body.b * k, 1f);
                });

                // lower half in shade
                t.Paint((x, y) => Mathf.Max(bodySdf(x, y), y - (cy + hh * 0.04f)), new Color(0f, 0f, 0f, 0.18f));

                // ---- livery -------------------------------------------------------
                if (shape == 1 || shape == 5)
                {
                    t.Paint(TexCanvas.Intersect(bodySdf, (x, y) => Mathf.Abs(x - cx) - w * 0.085f),
                            TexCanvas.A(accent, 0.45f));
                }
                if (shape == 4)
                {
                    t.Paint(TexCanvas.Intersect(bodySdf,
                            (x, y) => Mathf.Abs(Mathf.Abs(x - cx) - w * 0.15f) - w * 0.030f),
                            TexCanvas.A(accent, 0.55f));
                }
                if (shape == 2 || shape == 3)
                {
                    // centre stripe on the hood only
                    t.Paint(TexCanvas.Intersect(bodySdf, (x, y) => Mathf.Max(
                            Mathf.Abs(x - cx) - w * 0.055f, (cy + hh * 0.30f) - y)),
                            TexCanvas.A(accent, 0.5f));
                }

                // ---- roof, glass, panels -----------------------------------------
                var roof = (Func<float, float, float>)((x, y) =>
                    TexCanvas.RoundBox(x, y, cx, cy - hh * 0.04f, hw * 0.62f, hh * 0.30f, hw * 0.22f));
                t.Paint(roof, TexCanvas.Mul(body, 1.04f));
                t.Paint((x, y) => Mathf.Abs(roof(x, y) + 1.5f) - 0.8f, new Color(1f, 1f, 1f, 0.10f));

                // windshield: dark reflective glass at the front of the roof
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy + hh * 0.22f, hw * 0.52f, hh * 0.095f, hw * 0.16f),
                        new Color(0.08f, 0.14f, 0.22f, 0.95f));
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy + hh * 0.26f, hw * 0.44f, hh * 0.035f, hw * 0.10f),
                        new Color(0.55f, 0.80f, 1f, 0.30f));

                // rear window
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy - hh * 0.32f, hw * 0.46f, hh * 0.060f, hw * 0.13f),
                        new Color(0.07f, 0.12f, 0.19f, 0.95f));

                // hood + trunk shut lines and a centre crease
                t.Paint((x, y) => Mathf.Max(bodySdf(x, y), Mathf.Abs(y - (cy + hh * 0.72f)) - 1.1f),
                        new Color(0f, 0f, 0f, 0.22f));
                t.Paint((x, y) => Mathf.Max(bodySdf(x, y), Mathf.Abs(y - (cy - hh * 0.54f)) - 1.1f),
                        new Color(0f, 0f, 0f, 0.22f));
                t.Paint((x, y) => Mathf.Max(bodySdf(x, y),
                        Mathf.Max(Mathf.Abs(x - cx) - 1.1f, (cy + hh * 0.28f) - y)), new Color(0f, 0f, 0f, 0.16f));

                // ---- lights -------------------------------------------------------
                if (player)
                {
                    foreach (var s in new[] { -1f, 1f })
                    {
                        float lx = cx + s * hw * 0.40f;
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, lx, cy + hh * 0.80f, w * 0.10f, h * 0.010f, 3f),
                                new Color(0.95f, 0.99f, 1f, 1f));
                        t.Glow((x, y) => TexCanvas.RoundBox(x, y, lx, cy + hh * 0.80f, w * 0.11f, h * 0.022f, 7f),
                               glow, 0.30f, 4f);
                    }
                }

                var tail = player ? new Color(1f, 0.30f, 0.26f, 1f) : new Color(1f, 0.18f, 0.14f, 1f);
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy - hh * 0.86f, hw * 0.62f, h * 0.009f, 3f), tail);
                foreach (var s in new[] { -1f, 1f })
                {
                    float lx = cx + s * hw * 0.52f;
                    t.Glow((x, y) => TexCanvas.Circle(x, y, lx, cy - hh * 0.86f, w * 0.085f),
                           new Color(1f, 0.25f, 0.15f, 1f), player ? 0.55f : 1.2f, 2.8f);
                }

                // ---- neon outline -------------------------------------------------
                t.Paint((x, y) => Mathf.Abs(bodySdf(x, y)) - w * 0.0085f, TexCanvas.A(accent, player ? 0.95f : 0.55f));

                // ---- wheels and mirrors on top ------------------------------------
                var tyre = new Color(0.05f, 0.055f, 0.07f, 1f);
                float wheelW = w * 0.052f, wheelH = h * 0.100f;
                float wheelX = hw * 0.99f;
                foreach (var sy in new[] { cy + hh * 0.56f, cy - hh * 0.56f })
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        float wx = cx + s * wheelX;
                        float wy = sy;
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, wx, wy, wheelW, wheelH, wheelW * 0.55f), tyre);
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, wx, wy + wheelH * 0.12f, wheelW * 0.42f, wheelH * 0.26f, 2f),
                                new Color(0.26f, 0.29f, 0.36f, 0.85f));
                    }
                }

                foreach (var s in new[] { -1f, 1f })
                {
                    float mx = cx + s * hw * 1.02f;
                    t.Paint((x, y) => TexCanvas.RoundBox(x, y, mx, cy + hh * 0.20f, w * 0.028f, h * 0.014f, 3f),
                            TexCanvas.Mul(body, 0.75f));
                }

                // ---- shape flourishes ---------------------------------------------
                if (shape == 0 || shape == 4)
                {
                    t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy - hh * 0.84f, hw * 0.86f, h * 0.011f, 3f),
                            TexCanvas.Mul(accent, 0.9f));
                }
                if (shape == 2)
                {
                    t.Paint((x, y) => TexCanvas.Triangle(x, y, cx, cy + hh * 0.62f,
                            cx - hw * 0.09f, cy - hh * 0.18f, cx + hw * 0.09f, cy - hh * 0.18f),
                            TexCanvas.A(accent, 0.30f));
                }
                if (mover)
                {
                    var amber = new Color(1f, 0.72f, 0.16f, 0.95f);
                    t.Paint((x, y) => TexCanvas.Triangle(x, y, cx - hw * 0.26f, cy - hh * 0.02f,
                            cx - hw * 0.08f, cy + hh * 0.10f, cx - hw * 0.26f, cy + hh * 0.22f), amber);
                    t.Paint((x, y) => TexCanvas.Triangle(x, y, cx + hw * 0.26f, cy - hh * 0.02f,
                            cx + hw * 0.08f, cy + hh * 0.10f, cx + hw * 0.26f, cy + hh * 0.22f), amber);
                }
            });
        }

        public static Sprite Car(SkinDef skin) =>
            Get("car:" + skin.id, () => MakeCar(skin.body, skin.accent, skin.glow, skin.shape, true));

        public static Sprite TrafficCar(int variant)
        {
            Color body, accent;
            switch (variant % 3)
            {
                case 0: body = new Color(0.74f, 0.16f, 0.16f); accent = new Color(1f, 0.55f, 0.45f); break;
                case 1: body = new Color(0.78f, 0.36f, 0.10f); accent = new Color(1f, 0.72f, 0.40f); break;
                default: body = new Color(0.72f, 0.58f, 0.12f); accent = new Color(1f, 0.88f, 0.50f); break;
            }
            return Get("traffic:" + (variant % 3), () => MakeCar(body, accent, new Color(1f, 0.4f, 0.2f), 0, false));
        }

        public static Sprite MoverCar() =>
            Get("mover", () => MakeCar(new Color(0.48f, 0.20f, 0.62f), new Color(0.92f, 0.72f, 1f),
                                        new Color(0.85f, 0.55f, 1f), 1, false, true));

        public static Sprite Truck(int variant)
        {
            return Get("truck:" + (variant % 2), () =>
            {
                int w = Px(Layout.TruckW), h = Px(Layout.TruckH);
                bool tanker = (variant % 2) == 1;
                return Make(w, h, t =>
                {
                    float cx = w * 0.5f, cy = h * 0.5f;
                    float hw = w * 0.42f, hh = h * 0.47f;

                    t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy - h * 0.01f, hw * 1.02f, hh * 0.99f, hw * 0.22f),
                            new Color(0, 0, 0, 0.55f), 1f, 8f);

                    var tyre = new Color(0.05f, 0.055f, 0.07f, 1f);
                    for (int i = 0; i < 3; i++)
                    {
                        float wy = cy + hh * (0.55f - i * 0.52f);
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx - hw - w * 0.03f, wy, w * 0.045f, h * 0.055f, 3f), tyre);
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx + hw + w * 0.03f, wy, w * 0.045f, h * 0.055f, 3f), tyre);
                    }

                    var bodySdf = (Func<float, float, float>)((x, y) => TexCanvas.RoundBox(x, y, cx, cy, hw, hh, hw * 0.16f));
                    var baseCol = tanker ? new Color(0.72f, 0.44f, 0.10f) : new Color(0.34f, 0.40f, 0.50f);
                    t.PaintMasked(bodySdf, (x, y) =>
                    {
                        float v = Mathf.InverseLerp(cy - hh, cy + hh, y);
                        float k = Mathf.Lerp(0.7f, 1.05f, v);
                        return new Color(baseCol.r * k, baseCol.g * k, baseCol.b * k, 1f);
                    });

                    // cab
                    var cabSdf = (Func<float, float, float>)((x, y) =>
                        TexCanvas.RoundBox(x, y, cx, cy + hh * 0.70f, hw * 0.98f, hh * 0.20f, hw * 0.18f));
                    t.Paint(cabSdf, new Color(0.16f, 0.18f, 0.24f, 1f));
                    t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy + hh * 0.74f, hw * 0.80f, hh * 0.075f, 4f),
                            new Color(0.42f, 0.58f, 0.70f, 0.95f));
                    t.Paint(TexCanvas.Intersect(bodySdf, (x, y) => Mathf.Abs(y - (cy + hh * 0.49f)) - h * 0.004f),
                            new Color(0, 0, 0, 0.55f));

                    if (!tanker)
                    {
                        // container ribs
                        for (int i = 0; i < 7; i++)
                        {
                            float ry = cy - hh * 0.82f + i * hh * 0.21f;
                            t.Paint(TexCanvas.Intersect(bodySdf, (x, y) => Mathf.Abs(y - ry) - h * 0.004f),
                                    new Color(0, 0, 0, 0.18f));
                        }
                    }
                    else
                    {
                        t.Paint(TexCanvas.Intersect(bodySdf, (x, y) => Mathf.Abs(y - cy) - h * 0.004f),
                                new Color(1f, 1f, 1f, 0.10f));
                    }

                    t.Paint((x, y) => Mathf.Abs(bodySdf(x, y)) - w * 0.008f, new Color(1f, 0.62f, 0.24f, 0.55f));

                    foreach (var sx in new[] { -1f, 1f })
                    {
                        float lx = cx + sx * hw * 0.62f;
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, lx, cy - hh * 0.94f, w * 0.09f, h * 0.013f, 2f),
                                new Color(1f, 0.20f, 0.14f, 1f));
                        t.Glow((x, y) => TexCanvas.Circle(x, y, lx, cy - hh * 0.94f, w * 0.085f),
                               new Color(1f, 0.28f, 0.16f, 1f), 1.1f, 2.6f);
                    }
                });
            });
        }

        public static Sprite Barrier()
        {
            return Get("barrier", () =>
            {
                int w = Px(Layout.BarrierW), h = Px(Layout.BarrierH);
                return Make(w, h, t =>
                {
                    float cx = w * 0.5f, cy = h * 0.5f;
                    float hw = w * 0.46f, hh = h * 0.20f;

                    t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy - 6, hw * 1.02f, hh * 1.25f, 8f), new Color(0, 0, 0, 0.5f), 1f, 7f);

                    // feet
                    foreach (var sx in new[] { -1f, 1f })
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx + sx * hw * 0.72f, cy - hh * 0.95f, w * 0.06f, h * 0.16f, 3f),
                                new Color(0.16f, 0.17f, 0.20f, 1f));

                    var bar = (Func<float, float, float>)((x, y) => TexCanvas.RoundBox(x, y, cx, cy + hh * 0.15f, hw, hh, 6f));
                    t.Paint(bar, new Color(0.14f, 0.14f, 0.16f, 1f));

                    // diagonal hazard stripes, clipped to the bar
                    var stripes = (Func<float, float, float>)((x, y) =>
                    {
                        float period = 34f;
                        float s = ((x - y * 0.55f) % period + period) % period;
                        return s - period * 0.5f;
                    });
                    t.Paint(TexCanvas.Intersect(bar, stripes), new Color(1f, 0.78f, 0.14f, 1f));

                    t.Paint((x, y) => Mathf.Abs(bar(x, y)) - 2.2f, new Color(1f, 0.62f, 0.16f, 0.9f));
                    t.Glow((x, y) => bar(x, y), new Color(1f, 0.5f, 0.1f, 1f), 0.5f, 6f);
                });
            });
        }

        public static Sprite ArrowWarning() =>
            Get("arrowWarning", () => Make(64, 64, t =>
            {
                t.Paint((x, y) => TexCanvas.Triangle(x, y, 20, 32, 46, 50, 46, 14), new Color(1f, 0.72f, 0.2f, 1f));
            }, UI_PPU));

        // ================================================================== props
        public static Sprite Coin() =>
            Get("coin", () => Make(96, 96, t =>
            {
                float cx = 48, cy = 48;
                t.Paint((x, y) => TexCanvas.Circle(x, y, cx, cy, 44), new Color(1f, 0.80f, 0.24f, 1f));
                t.Paint((x, y) => TexCanvas.Circle(x, y, cx, cy, 38), new Color(1f, 0.68f, 0.10f, 1f));
                t.Paint((x, y) => TexCanvas.Ring(x, y, cx, cy, 33, 4), new Color(1f, 0.92f, 0.60f, 1f));
                // 4 point sparkle
                var v1 = (Func<float, float, float>)((x, y) => Mathf.Abs(x - cx) * 3.2f + Mathf.Abs(y - cy) - 16f);
                var v2 = (Func<float, float, float>)((x, y) => Mathf.Abs(x - cx) + Mathf.Abs(y - cy) * 3.2f - 16f);
                t.Paint(TexCanvas.Union(v1, v2), new Color(1f, 0.96f, 0.78f, 0.95f));
                t.Paint((x, y) => TexCanvas.Circle(x, y, cx - 14, cy + 16, 5), new Color(1f, 1f, 0.9f, 0.55f), 1f, 2.5f);
            }, PPU));

        public static Sprite Glow()
        {
            return Get("glow", () => Make(128, 128, t =>
            {
                t.RadialGlow(64, 64, 62, Color.white, 2.2f, 1f);
            }, PPU));
        }

        public static Sprite SoftDot()
        {
            return Get("dot", () => Make(32, 32, t =>
            {
                t.RadialGlow(16, 16, 15, Color.white, 1.6f, 1f);
            }, PPU));
        }

        public static Sprite Beam()
        {
            return Get("beam", () => Make(160, 300, t =>
            {
                for (int y = 0; y < t.H; y++)
                {
                    float v = y / (float)t.H;
                    float halfW = Mathf.Lerp(8f, 84f, v * v);
                    float a = (1f - v) * 0.55f;
                    for (int x = 0; x < t.W; x++)
                    {
                        float dx = Mathf.Abs(x + 0.5f - 80f);
                        float edge = Mathf.Clamp01((halfW - dx) / Mathf.Max(1f, halfW * 0.9f));
                        if (edge <= 0f) continue;
                        t.Blend(x, y, Color.white, a * edge * edge);
                    }
                }
            }, PPU));
        }

        public static Sprite Shadow() =>
            Get("shadow", () => Make(128, 128, t =>
            {
                t.RadialGlow(64, 64, 62, Color.white, 1.4f, 1f);
            }, PPU));

        public static Sprite Decor(int style)
        {
            return Get("decor:" + style, () =>
            {
                if (style == 1)
                {
                    // tunnel rib with lights
                    return Make(220, 60, t =>
                    {
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, 110, 30, 106, 12, 6), new Color(0.12f, 0.10f, 0.09f, 1f));
                        for (int i = 0; i < 5; i++)
                        {
                            float lx = 24 + i * 43;
                            t.Paint((x, y) => TexCanvas.Circle(x, y, lx, 30, 7), new Color(1f, 0.78f, 0.42f, 1f));
                            t.Glow((x, y) => TexCanvas.Circle(x, y, lx, 30, 26), new Color(1f, 0.62f, 0.24f, 1f), 0.55f, 9f);
                        }
                    }, PPU);
                }
                if (style == 2)
                {
                    // desert rock / cactus
                    return Make(90, 110, t =>
                    {
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, 45, 34, 30, 30, 14), new Color(0.20f, 0.12f, 0.12f, 1f));
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, 45, 62, 10, 26, 9), new Color(0.18f, 0.13f, 0.10f, 1f));
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, 30, 66, 8, 12, 6), new Color(0.18f, 0.13f, 0.10f, 1f));
                        t.Paint((x, y) => TexCanvas.RoundBox(x, y, 60, 56, 8, 16, 6), new Color(0.18f, 0.13f, 0.10f, 1f));
                        t.Glow((x, y) => TexCanvas.RoundBox(x, y, 45, 34, 30, 30, 14), new Color(1f, 0.5f, 0.2f, 1f), 0.35f, 10f);
                    }, PPU);
                }
                // city lamp post
                return Make(70, 150, t =>
                {
                    t.Paint((x, y) => TexCanvas.RoundBox(x, y, 35, 70, 4, 66, 3), new Color(0.13f, 0.14f, 0.20f, 1f));
                    t.Paint((x, y) => TexCanvas.RoundBox(x, y, 35, 138, 20, 8, 6), new Color(0.12f, 0.13f, 0.18f, 1f));
                    t.Paint((x, y) => TexCanvas.RoundBox(x, y, 35, 132, 15, 4, 2), new Color(0.85f, 0.96f, 1f, 1f));
                    t.Glow((x, y) => TexCanvas.Circle(x, y, 35, 128, 40), new Color(0.45f, 0.85f, 1f, 1f), 0.55f, 14f);
                    t.Glow((x, y) => TexCanvas.Circle(x, y, 35, 86, 55), new Color(0.3f, 0.6f, 1f, 1f), 0.22f, 16f);
                }, PPU);
            });
        }

        // ============================================================== textures
        static Texture2D MakeTexture(int w, int h, Action<TexCanvas> draw, TextureWrapMode wrap)
        {
            var c = new TexCanvas(w, h);
            draw(c);
            return c.ToTexture(wrap);
        }

        /// <summary>Road surface + shoulders + lane markings. Tiles vertically.</summary>
        public static Texture2D RoadTexture(ThemeDef theme)
        {
            return GetTex("road:" + theme.id, () =>
            {
                const float ppu = 40f;
                int shoulderPx = Mathf.RoundToInt(0.62f * ppu);
                int roadPx = Mathf.RoundToInt(Layout.RoadHalf * 2f * ppu);
                int w = roadPx + shoulderPx * 2;
                int h = Mathf.RoundToInt(4f * ppu);      // 2 units of dash + 2 units of gap

                return MakeTexture(w, h, t =>
                {
                    var rnd = new System.Random(theme.id.GetHashCode());
                    float edgeL = shoulderPx;
                    float edgeR = w - shoulderPx;
                    float lane1 = shoulderPx + roadPx / 3f;
                    float lane2 = shoulderPx + roadPx * 2f / 3f;
                    float laneW = roadPx / 3f;
                    float dashLen = 2f * ppu;

                    // --- asphalt / shoulder with grain and a soft sheen down each lane
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            bool shoulder = x < shoulderPx || x >= w - shoulderPx;
                            var c = shoulder ? theme.shoulder : theme.road;
                            float n = ((float)rnd.NextDouble() - 0.5f) * 0.05f;

                            if (!shoulder)
                            {
                                float laneCentre = shoulderPx + laneW * 0.5f + laneW * Mathf.Floor((x - shoulderPx) / laneW);
                                float d = Mathf.Abs(x + 0.5f - laneCentre) / (laneW * 0.5f);
                                float sheen = Mathf.Pow(1f - Mathf.Clamp01(d), 2.5f) * 0.055f;
                                c = new Color(c.r + sheen, c.g + sheen, c.b + sheen * 1.15f, 1f);
                            }

                            t.Px[y * w + x] = new Color(c.r + n, c.g + n, c.b + n, 1f);
                        }
                    }

                    // --- dashes
                    for (int lane = 1; lane < 3; lane++)
                    {
                        float lx = ShoulderLaneX(lane, shoulderPx, laneW);
                        for (int y = h - (int)dashLen; y < h; y++)
                        {
                            for (int x = (int)(lx - 3.5f); x <= (int)(lx + 3.5f); x++)
                                t.Blend(x, y, theme.laneLine, 0.95f);
                        }
                    }

                    // --- solid edge lines
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = (int)(edgeL - 4); x <= (int)edgeL; x++)
                            t.Blend(x, y, theme.laneLine, 0.85f);
                        for (int x = (int)edgeR; x <= (int)(edgeR + 4); x++)
                            t.Blend(x, y, theme.laneLine, 0.85f);
                    }

                    // --- neon bloom around every line
                    for (int y = 0; y < h; y++)
                    {
                        bool dashOn = y >= h - dashLen;
                        for (int x = 0; x < w; x++)
                        {
                            float g = 0f;
                            g += LineGlow(x, edgeL, 16f) * 0.55f;
                            g += LineGlow(x, edgeR, 16f) * 0.55f;
                            if (dashOn)
                            {
                                g += LineGlow(x, lane1, 13f) * 0.5f;
                                g += LineGlow(x, lane2, 13f) * 0.5f;
                            }
                            else
                            {
                                g += LineGlow(x, lane1, 13f) * 0.12f;
                                g += LineGlow(x, lane2, 13f) * 0.12f;
                            }
                            if (g <= 0.001f) continue;
                            var dst = t.Px[y * w + x];
                            t.Px[y * w + x] = new Color(dst.r + theme.laneLine.r * g,
                                                        dst.g + theme.laneLine.g * g,
                                                        dst.b + theme.laneLine.b * g, 1f);
                        }
                    }
                }, TextureWrapMode.Repeat);
            });
        }

        static float ShoulderLaneX(int lane, int shoulderPx, float laneW)
            => shoulderPx + laneW * lane;

        static float LineGlow(float x, float lineX, float radius)
        {
            float d = Mathf.Abs(x + 0.5f - lineX) / radius;
            if (d >= 1f) return 0f;
            float k = 1f - d;
            return k * k * 0.35f;
        }

        public static Texture2D SpeedLines(ThemeDef theme)
        {
            return GetTex("speed:" + theme.id, () =>
            {
                int w = 64, h = 256;
                return MakeTexture(w, h, t =>
                {
                    var rnd = new System.Random(17);
                    for (int i = 0; i < 7; i++)
                    {
                        int x = rnd.Next(2, w - 3);
                        int len = rnd.Next(40, 150);
                        int y0 = rnd.Next(0, h - len - 1);
                        float a = 0.20f + (float)rnd.NextDouble() * 0.35f;
                        for (int y = y0; y < y0 + len; y++)
                        {
                            float fade = Mathf.Sin(Mathf.PI * (y - y0) / len);
                            t.Blend(x, y, theme.laneLine, a * fade);
                            t.Blend(x + 1, y, theme.laneLine, a * fade * 0.5f);
                        }
                    }
                }, TextureWrapMode.Repeat);
            });
        }

        public static Texture2D Starfield()
        {
            return GetTex("stars", () =>
            {
                int s = 256;
                return MakeTexture(s, s, t =>
                {
                    var rnd = new System.Random(99);
                    for (int i = 0; i < 160; i++)
                    {
                        int x = rnd.Next(0, s), y = rnd.Next(0, s);
                        float a = 0.25f + (float)rnd.NextDouble() * 0.75f;
                        float r = (float)rnd.NextDouble() * 1.6f + 0.5f;
                        t.Paint((px, py) => TexCanvas.Circle(px, py, x, y, r), new Color(0.85f, 0.92f, 1f, a), 1f, 1.6f);
                    }
                    for (int i = 0; i < 12; i++)
                        t.RadialGlow(rnd.Next(0, s), rnd.Next(0, s), 5f, new Color(0.7f, 0.85f, 1f, 1f), 2f, 0.5f);
                }, TextureWrapMode.Repeat);
            });
        }

        public static Texture2D Vignette()
        {
            return GetTex("vignette", () =>
            {
                int s = 256;
                var tex = new Texture2D(s, s, TextureFormat.RGBA32, false, false);
                var px = new Color[s * s];
                for (int y = 0; y < s; y++)
                    for (int x = 0; x < s; x++)
                    {
                        float nx = (x + 0.5f) / s * 2f - 1f;
                        float ny = (y + 0.5f) / s * 2f - 1f;
                        float r = Mathf.Sqrt(nx * nx * 0.72f + ny * ny * 0.92f);
                        float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((r - 0.62f) / 0.55f)) * 0.85f;
                        px[y * s + x] = new Color(0f, 0f, 0f, a);
                    }
                tex.SetPixels(px);
                tex.Apply(false, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            });
        }

        public static Texture2D Fog(float heightUnits, ThemeDef theme)
        {
            return GetTex("fog:" + theme.id, () =>
            {
                int w = 8;
                int h = Mathf.RoundToInt(heightUnits * 40f);
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                var px = new Color[w * h];
                for (int y = 0; y < h; y++)
                {
                    float v = y / (float)(h - 1);
                    float a = Mathf.Pow(v, 2.2f) * 0.9f;
                    for (int x = 0; x < w; x++) px[y * w + x] = new Color(theme.skyBottom.r, theme.skyBottom.g, theme.skyBottom.b, a);
                }
                tex.SetPixels(px);
                tex.Apply(false, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            });
        }

        public static Texture2D SkyGradient(ThemeDef theme)
        {
            return GetTex("sky:" + theme.id, () =>
            {
                int w = 8, h = 128;
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                var px = new Color[w * h];
                for (int y = 0; y < h; y++)
                {
                    float v = y / (float)(h - 1);
                    var c = Color.Lerp(theme.skyBottom, theme.skyTop, Mathf.Pow(v, 1.3f));
                    for (int x = 0; x < w; x++) px[y * w + x] = c;
                }
                tex.SetPixels(px);
                tex.Apply(false, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            });
        }

        // ============================================================ UI surfaces
        public static Sprite PanelFill() =>
            Get("panelFill", () => Make(96, 96, t =>
            {
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, 48, 48, 47, 47, 26), Color.white);
            }, UI_PPU, new Vector4(32, 32, 32, 32)));

        public static Sprite PanelRing() =>
            Get("panelRing", () => Make(96, 96, t =>
            {
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, 48, 48, 44, 44, 24), Color.white);
                t.Cut((x, y) => TexCanvas.RoundBox(x, y, 48, 48, 41.5f, 41.5f, 21.5f));
            }, UI_PPU, new Vector4(32, 32, 32, 32)));

        public static Sprite Pill() =>
            Get("pill", () => Make(96, 96, t =>
            {
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, 48, 48, 47, 47, 48), Color.white);
            }, UI_PPU, new Vector4(40, 40, 40, 40)));

        public static Sprite White() =>
            Get("white", () => Make(8, 8, t => t.Rectangle(0, 0, 7, 7, Color.white), UI_PPU));

        public static Sprite CircleSolid() =>
            Get("circle", () => Make(96, 96, t => t.Paint((x, y) => TexCanvas.Circle(x, y, 48, 48, 47), Color.white), UI_PPU));

        public static Sprite ProgressBar() => White();

        // ================================================================= icons
        static Sprite Icon(string key, Action<TexCanvas> draw) => Get("icon:" + key, () => Make(64, 64, draw, UI_PPU));

        public static Sprite IconPause() => Icon("pause", t =>
        {
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 24, 32, 5, 18, 2.5f), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 40, 32, 5, 18, 2.5f), Color.white);
        });

        public static Sprite IconPlay() => Icon("play", t =>
        {
            t.Paint((x, y) => TexCanvas.Triangle(x, y, 20, 12, 20, 52, 52, 32), Color.white);
        });

        public static Sprite IconHome() => Icon("home", t =>
        {
            t.Paint((x, y) => TexCanvas.Triangle(x, y, 12, 32, 32, 54, 52, 32), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 22, 17, 12, 3), Color.white);
            t.Cut((x, y) => TexCanvas.RoundBox(x, y, 32, 16, 6, 7, 1));
        });

        public static Sprite IconRetry() => Icon("retry", t =>
        {
            t.Paint((x, y) => TexCanvas.Ring(x, y, 32, 32, 18, 6), Color.white);
            t.Paint((x, y) => TexCanvas.Triangle(x, y, 46, 42, 58, 40, 52, 54), Color.white);
            t.Cut((x, y) => TexCanvas.RoundBox(x, y, 32, 20, 12, 12, 8));
        });

        public static Sprite IconClose() => Icon("close", t =>
        {
            t.Paint((x, y) => TexCanvas.Segment(x, y, 20, 20, 44, 44, 7), Color.white);
            t.Paint((x, y) => TexCanvas.Segment(x, y, 44, 20, 20, 44, 7), Color.white);
        });

        public static Sprite IconGear() => Icon("gear", t =>
        {
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                float cx = 32 + Mathf.Cos(a) * 18f, cy = 32 + Mathf.Sin(a) * 18f;
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, cx, cy, 7, 7, 2), Color.white);
            }
            t.Paint((x, y) => TexCanvas.Circle(x, y, 32, 32, 18), Color.white);
            t.Cut((x, y) => TexCanvas.Circle(x, y, 32, 32, 8));
        });

        public static Sprite IconGift() => Icon("gift", t =>
        {
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 24, 20, 14, 4), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 44, 20, 7, 3), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 24, 4, 14, 0), new Color(0.35f, 0.35f, 0.45f, 1f));
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 44, 4, 7, 0), new Color(0.35f, 0.35f, 0.45f, 1f));
            t.Paint((x, y) => TexCanvas.Ring(x, y, 22, 50, 8, 5), Color.white);
            t.Paint((x, y) => TexCanvas.Ring(x, y, 42, 50, 8, 5), Color.white);
        });

        public static Sprite IconCoin() => Icon("coin", t =>
        {
            t.Paint((x, y) => TexCanvas.Circle(x, y, 32, 32, 22), Color.white);
            t.Paint((x, y) => TexCanvas.Circle(x, y, 32, 32, 15), new Color(0.55f, 0.42f, 0.1f, 1f));
        });

        public static Sprite IconLock() => Icon("lock", t =>
        {
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 24, 17, 14, 4), Color.white);
            t.Paint((x, y) => TexCanvas.Ring(x, y, 32, 40, 11, 6), Color.white);
            t.Paint((x, y) => TexCanvas.Circle(x, y, 32, 26, 4), new Color(0.4f, 0.4f, 0.5f, 1f));
        });

        public static Sprite IconCar() => Icon("car", t =>
        {
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 30, 14, 22, 8), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 34, 9, 10, 5), new Color(0.4f, 0.4f, 0.5f, 1f));
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 18, 30, 5, 8, 2), new Color(0.4f, 0.4f, 0.5f, 1f));
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 46, 30, 5, 8, 2), new Color(0.4f, 0.4f, 0.5f, 1f));
        });

        public static Sprite IconList() => Icon("list", t =>
        {
            for (int i = 0; i < 3; i++)
            {
                float y = 18 + i * 14;
                t.Paint((px, py) => TexCanvas.RoundBox(px, py, 34, y, 20, 3.2f, 1.5f), Color.white);
                t.Paint((px, py) => TexCanvas.Circle(px, py, 14, y, 4.5f), Color.white);
            }
        });

        public static Sprite IconVideo() => Icon("video", t =>
        {
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 32, 24, 17, 6), Color.white);
            t.Paint((x, y) => TexCanvas.Triangle(x, y, 26, 22, 26, 42, 44, 32), new Color(0.25f, 0.25f, 0.35f, 1f));
        });

        public static Sprite IconCheck() => Icon("check", t =>
        {
            t.Paint((x, y) => TexCanvas.Segment(x, y, 16, 32, 27, 20, 8), Color.white);
            t.Paint((x, y) => TexCanvas.Segment(x, y, 27, 20, 49, 46, 8), Color.white);
        });

        public static Sprite IconTrophy() => Icon("trophy", t =>
        {
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 38, 15, 16, 6), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 20, 4, 8, 2), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 12, 13, 4, 2), Color.white);
            t.Paint((x, y) => TexCanvas.Ring(x, y, 13, 42, 8, 4), Color.white);
            t.Paint((x, y) => TexCanvas.Ring(x, y, 51, 42, 8, 4), Color.white);
        });

        public static Sprite IconArrowRight() => Icon("arrowR", t =>
        {
            t.Paint((x, y) => TexCanvas.Segment(x, y, 22, 20, 42, 32, 7), Color.white);
            t.Paint((x, y) => TexCanvas.Segment(x, y, 42, 32, 22, 44, 7), Color.white);
        });

        public static Sprite IconSound(bool on) => Icon(on ? "soundOn" : "soundOff", t =>
        {
            t.Paint((x, y) => TexCanvas.Triangle(x, y, 14, 26, 14, 38, 26, 44), Color.white);
            t.Paint((x, y) => TexCanvas.Triangle(x, y, 14, 26, 26, 20, 26, 44), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 22, 32, 8, 12, 2), Color.white);
            if (on)
            {
                t.Paint((x, y) => TexCanvas.Ring(x, y, 24, 32, 16, 4), Color.white);
                t.Paint((x, y) => TexCanvas.Ring(x, y, 24, 32, 24, 4), Color.white);
            }
            else
            {
                t.Paint((x, y) => TexCanvas.Segment(x, y, 36, 22, 50, 42, 6), Color.white);
                t.Paint((x, y) => TexCanvas.Segment(x, y, 50, 22, 36, 42, 6), Color.white);
            }
        });

        public static Sprite IconVibrate(bool on) => Icon(on ? "vibOn" : "vibOff", t =>
        {
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 32, 32, 11, 20, 4), Color.white);
            if (on)
            {
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, 12, 32, 3, 10, 1.5f), Color.white);
                t.Paint((x, y) => TexCanvas.RoundBox(x, y, 52, 32, 3, 10, 1.5f), Color.white);
            }
            else
            {
                t.Paint((x, y) => TexCanvas.Segment(x, y, 10, 14, 54, 50, 5), Color.white);
            }
        });

        public static Sprite IconMusic(bool on) => Icon(on ? "musicOn" : "musicOff", t =>
        {
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 24, 20, 9, 7, 3.5f), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 46, 26, 9, 7, 3.5f), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 31, 38, 3, 16, 1.5f), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 53, 44, 3, 16, 1.5f), Color.white);
            t.Paint((x, y) => TexCanvas.RoundBox(x, y, 42, 54, 14, 3, 1.5f), Color.white);
            if (!on) t.Paint((x, y) => TexCanvas.Segment(x, y, 8, 10, 56, 54, 5), Color.white);
        });

        public static Sprite IconShare() => Icon("share", t =>
        {
            t.Paint((x, y) => TexCanvas.Circle(x, y, 18, 12, 7), Color.white);
            t.Paint((x, y) => TexCanvas.Circle(x, y, 46, 30, 7), Color.white);
            t.Paint((x, y) => TexCanvas.Circle(x, y, 18, 50, 7), Color.white);
            t.Paint((x, y) => TexCanvas.Segment(x, y, 22, 15, 42, 27, 4), Color.white);
            t.Paint((x, y) => TexCanvas.Segment(x, y, 22, 47, 42, 33, 4), Color.white);
        });
    }
}

