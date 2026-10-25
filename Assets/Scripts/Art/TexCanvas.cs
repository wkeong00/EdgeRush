using System;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>
    /// Tiny software rasteriser used to build every sprite in the game at runtime.
    /// Coordinates are in pixels with (0,0) at the bottom-left, matching Texture2D.
    /// </summary>
    public class TexCanvas
    {
        public readonly int W, H;
        public readonly Color[] Px;

        public TexCanvas(int w, int h)
        {
            W = w; H = h;
            Px = new Color[w * h];
            var clear = new Color(0, 0, 0, 0);
            for (int i = 0; i < Px.Length; i++) Px[i] = clear;
        }

        public void Blend(int x, int y, Color c, float a)
        {
            if (a <= 0f || x < 0 || y < 0 || x >= W || y >= H) return;
            int i = y * W + x;
            var dst = Px[i];
            if (a >= 1f) { Px[i] = new Color(c.r, c.g, c.b, c.a); return; }
            float outA = a + dst.a * (1f - a);
            if (outA <= 0f) { Px[i] = new Color(0, 0, 0, 0); return; }
            float r = (c.r * a + dst.r * dst.a * (1f - a)) / outA;
            float g = (c.g * a + dst.g * dst.a * (1f - a)) / outA;
            float b = (c.b * a + dst.b * dst.a * (1f - a)) / outA;
            Px[i] = new Color(r, g, b, outA);
        }

        public void Add(int x, int y, Color c, float a)
        {
            if (a <= 0f || x < 0 || y < 0 || x >= W || y >= H) return;
            int i = y * W + x;
            var dst = Px[i];
            Px[i] = new Color(dst.r + c.r * a * c.a, dst.g + c.g * a * c.a, dst.b + c.b * a * c.a,
                              Mathf.Clamp01(dst.a + a * c.a));
        }

        /// <summary>Fill an implicit-surface shape (negative inside) with 1px antialiasing.</summary>
        public void Paint(Func<float, float, float> sdf, Color c, float alphaMul = 1f, float feather = 1.15f)
        {
            if (alphaMul <= 0f) return;
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float d = sdf(x + 0.5f, y + 0.5f);
                    float cov = Mathf.Clamp01(0.5f - d / feather);
                    if (cov <= 0f) continue;
                    Blend(x, y, c, cov * alphaMul * c.a);
                }
            }
        }

        /// <summary>Fill a shape using a per-pixel colour function (gradients, stripes).</summary>
        public void PaintMasked(Func<float, float, float> sdf, Func<int, int, Color> colorAt, float alphaMul = 1f, float feather = 1.15f)
        {
            if (alphaMul <= 0f) return;
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float d = sdf(x + 0.5f, y + 0.5f);
                    float cov = Mathf.Clamp01(0.5f - d / feather);
                    if (cov <= 0f) continue;
                    var c = colorAt(x, y);
                    Blend(x, y, c, cov * alphaMul * c.a);
                }
            }
        }

        /// <summary>Erase everything inside a shape (alpha 0).</summary>
        public void Cut(Func<float, float, float> sdf, float feather = 1.15f)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float d = sdf(x + 0.5f, y + 0.5f);
                    float cov = Mathf.Clamp01(0.5f - d / feather);
                    if (cov <= 0f) continue;
                    int i = y * W + x;
                    var c = Px[i];
                    Px[i] = new Color(c.r, c.g, c.b, c.a * (1f - cov));
                }
        }

        /// <summary>Additive fill of an implicit-surface shape (used for neon glow).</summary>
        public void Glow(Func<float, float, float> sdf, Color c, float alphaMul = 1f, float softness = 1.5f)
        {
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float d = sdf(x + 0.5f, y + 0.5f);
                    float a = Mathf.Clamp01(-d / softness);
                    if (a <= 0f) continue;
                    a = a * a;
                    Add(x, y, c, a * alphaMul);
                }
            }
        }

        public void RadialGlow(float cx, float cy, float radius, Color c, float power = 2f, float alphaMul = 1f)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - radius)), x1 = Mathf.Min(W - 1, Mathf.CeilToInt(cx + radius));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - radius)), y1 = Mathf.Min(H - 1, Mathf.CeilToInt(cy + radius));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    float t = 1f - Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    if (t <= 0f) continue;
                    Add(x, y, c, Mathf.Pow(t, power) * alphaMul);
                }
            }
        }

        public void VGradient(Color bottom, Color top, int yFrom = 0, int yTo = -1)
        {
            if (yTo < 0) yTo = H;
            for (int y = 0; y < H; y++)
            {
                float t = Mathf.Clamp01((y - yFrom) / Mathf.Max(1f, (float)(yTo - yFrom)));
                var c = Color.Lerp(bottom, top, t);
                for (int x = 0; x < W; x++)
                    Px[y * W + x] = c;
            }
        }

        public void HorizontalBand(float yCenter, float halfHeight, Color c, float alpha, float falloff = 1.6f)
        {
            for (int y = 0; y < H; y++)
            {
                float d = Mathf.Abs(y + 0.5f - yCenter);
                float t = 1f - Mathf.Clamp01((d - halfHeight * 0.35f) / Mathf.Max(1f, halfHeight));
                if (t <= 0f) continue;
                float a = Mathf.Pow(t, falloff) * alpha;
                for (int x = 0; x < W; x++) Add(x, y, c, a);
            }
        }

        public void Rectangle(int x0, int y0, int x1, int y1, Color c)
        {
            for (int y = Mathf.Max(0, y0); y <= Mathf.Min(H - 1, y1); y++)
                for (int x = Mathf.Max(0, x0); x <= Mathf.Min(W - 1, x1); x++)
                    Blend(x, y, c, c.a);
        }

        public Texture2D ToTexture(TextureWrapMode wrap = TextureWrapMode.Clamp, FilterMode filter = FilterMode.Bilinear)
        {
            var t = new Texture2D(W, H, TextureFormat.RGBA32, false, false);
            t.SetPixels(Px);
            t.Apply(false, false);
            t.wrapMode = wrap;
            t.filterMode = filter;
            return t;
        }

        // ------------------------------------------------------------------ SDFs
        public static float RoundBox(float x, float y, float cx, float cy, float hw, float hh, float r)
        {
            r = Mathf.Min(r, Mathf.Min(hw, hh));
            float dx = Mathf.Abs(x - cx) - (hw - r);
            float dy = Mathf.Abs(y - cy) - (hh - r);
            float ax = Mathf.Max(dx, 0f), ay = Mathf.Max(dy, 0f);
            return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(dx, dy), 0f) - r;
        }

        public static float Circle(float x, float y, float cx, float cy, float r)
        {
            float dx = x - cx, dy = y - cy;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        public static float Ring(float x, float y, float cx, float cy, float r, float thickness)
        {
            return Mathf.Abs(Circle(x, y, cx, cy, r)) - thickness * 0.5f;
        }

        public static float Segment(float x, float y, float ax, float ay, float bx, float by, float thickness)
        {
            float pax = x - ax, pay = y - ay, bax = bx - ax, bay = by - ay;
            float len2 = bax * bax + bay * bay;
            float h = len2 <= 0f ? 0f : Mathf.Clamp01((pax * bax + pay * bay) / len2);
            float dx = pax - bax * h, dy = pay - bay * h;
            return Mathf.Sqrt(dx * dx + dy * dy) - thickness * 0.5f;
        }

        public static float Triangle(float x, float y, float ax, float ay, float bx, float by, float cx, float cy)
        {
            // Signed distance to a triangle (negative inside).
            float e0 = Cross(ax, ay, bx, by, x, y);
            float e1 = Cross(bx, by, cx, cy, x, y);
            float e2 = Cross(cx, cy, ax, ay, x, y);
            bool inside = (e0 >= 0 && e1 >= 0 && e2 >= 0) || (e0 <= 0 && e1 <= 0 && e2 <= 0);
            float d = Mathf.Min(DistToSegment(x, y, ax, ay, bx, by),
                                Mathf.Min(DistToSegment(x, y, bx, by, cx, cy),
                                          DistToSegment(x, y, cx, cy, ax, ay)));
            return inside ? -d : d;
        }

        static float Cross(float ax, float ay, float bx, float by, float px, float py)
            => (bx - ax) * (py - ay) - (by - ay) * (px - ax);

        static float DistToSegment(float x, float y, float ax, float ay, float bx, float by)
        {
            float pax = x - ax, pay = y - ay, bax = bx - ax, bay = by - ay;
            float len2 = bax * bax + bay * bay;
            float h = len2 <= 0f ? 0f : Mathf.Clamp01((pax * bax + pay * bay) / len2);
            float dx = pax - bax * h, dy = pay - bay * h;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static Func<float, float, float> Intersect(Func<float, float, float> a, Func<float, float, float> b)
            => (x, y) => Mathf.Max(a(x, y), b(x, y));

        public static Func<float, float, float> Union(Func<float, float, float> a, Func<float, float, float> b)
            => (x, y) => Mathf.Min(a(x, y), b(x, y));

        public static Color Mul(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, c.a);
        public static Color A(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
