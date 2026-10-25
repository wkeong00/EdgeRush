using System.Collections.Generic;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>
    /// The scrolling road, background, decor and speed lines. Everything scrolls by moving
    /// texture offsets and re-cycling a small pool of decor sprites, so it stays cheap.
    /// </summary>
    public class RoadView : MonoBehaviour
    {
        const int DecorPerSide = 7;
        const float DecorSpacing = 10.5f;

        class Decor
        {
            public SpriteRenderer sr;
            public float y;
            public int side;
        }

        Camera _cam;
        Transform _root;

        MeshRenderer _roadRenderer;
        Material _roadMat;

        readonly List<MeshRenderer> _speedLines = new List<MeshRenderer>();
        readonly List<Material> _speedMats = new List<Material>();

        readonly List<Decor> _decor = new List<Decor>();
        SpriteRenderer _bg;
        SpriteRenderer _stars;
        SpriteRenderer _fog;
        SpriteRenderer _themeWash;

        ThemeDef _theme;
        int _themeIndex;
        float _washT = -1f;
        ThemeDef _pendingTheme;

        float _tileWorld;
        float _distance;

        public int ThemeIndex => _themeIndex;
        public ThemeDef Theme => _theme;

        static Shader SpriteShader
        {
            get
            {
                var s = Resources.Load<Shader>("Shaders/SpriteTint");
                if (s == null) s = Shader.Find("Sprites/Default");
                return s;
            }
        }

        static Shader AdditiveShader
        {
            get
            {
                var s = Resources.Load<Shader>("Shaders/Additive");
                if (s == null) s = Shader.Find("Sprites/Default");
                return s;
            }
        }

        static Mesh QuadMesh()
        {
            var m = new Mesh { name = "Quad" };
            m.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
            };
            m.uv = new[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            };
            m.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            m.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            m.RecalculateBounds();
            return m;
        }

        static Sprite TextureSprite(Texture2D tex, float ppu)
            => Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);

        static void Fit(SpriteRenderer sr, float worldW, float worldH)
        {
            var b = sr.sprite.bounds.size;
            sr.transform.localScale = new Vector3(worldW / b.x, worldH / b.y, 1f);
        }

        public void Build(Camera cam)
        {
            _cam = cam;
            _root = new GameObject("Road").transform;
            _root.SetParent(transform, false);
            _themeIndex = 0;
            _theme = Palette.Themes[0];

            BuildBackground();
            BuildRoad();
            BuildSpeedLines();
            BuildDecor();
            BuildOverlays();
        }

        void BuildBackground()
        {
            var bgGo = new GameObject("Sky", typeof(SpriteRenderer));
            bgGo.transform.SetParent(_root, false);
            _bg = bgGo.GetComponent<SpriteRenderer>();
            _bg.sprite = TextureSprite(Art.SkyGradient(_theme), 100f);
            _bg.sortingOrder = -100;
            bgGo.transform.position = new Vector3(0, 0, 8f);
            Fit(_bg, Layout.ViewHalfWidth * 2f + 2f, Layout.OrthoSize * 2f + 2f);

            var starsGo = new GameObject("Stars", typeof(SpriteRenderer));
            starsGo.transform.SetParent(_root, false);
            _stars = starsGo.GetComponent<SpriteRenderer>();
            _stars.sprite = TextureSprite(Art.Starfield(), 40f);
            _stars.sharedMaterial = new Material(AdditiveShader);
            _stars.sortingOrder = -99;
            _stars.transform.position = new Vector3(0, 0, 7f);
            Fit(_stars, Layout.ViewHalfWidth * 2f + 2f, Layout.OrthoSize * 2f + 2f);
            _stars.color = new Color(1f, 1f, 1f, _theme.starAlpha);
        }

        void BuildRoad()
        {
            var go = new GameObject("Surface", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(_root, false);
            go.transform.position = new Vector3(0, 0, 5f);
            go.GetComponent<MeshFilter>().sharedMesh = QuadMesh();
            _roadMat = new Material(SpriteShader) { name = "RoadMat" };
            _roadRenderer = go.GetComponent<MeshRenderer>();
            _roadRenderer.sharedMaterial = _roadMat;
            _roadRenderer.sortingOrder = 0;
            _roadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _roadRenderer.receiveShadows = false;

            ApplyThemeTextures(_theme, true);
            go.transform.localScale = new Vector3(Layout.RoadHalf * 2f + 1.24f, 70f, 1f);
        }

        void ApplyThemeTextures(ThemeDef theme, bool alsoColors)
        {
            var tex = Art.RoadTexture(theme);
            _roadMat.mainTexture = tex;
            _tileWorld = tex.height / 40f;
            _roadMat.mainTextureScale = new Vector2(1f, 70f / _tileWorld);
            _roadMat.mainTextureOffset = new Vector2(0f, _distance / _tileWorld);

            if (!alsoColors) return;
            if (_bg != null) _bg.sprite = TextureSprite(Art.SkyGradient(theme), 100f);
            if (_bg != null) Fit(_bg, Layout.ViewHalfWidth * 2f + 2f, Layout.OrthoSize * 2f + 2f);
            if (_fog != null) _fog.sprite = TextureSprite(Art.Fog(Layout.OrthoSize * 0.55f, theme), 40f);

            foreach (var d in _decor)
                if (d.sr != null) d.sr.sprite = Art.Decor(theme.decorStyle);

            foreach (var m in _speedMats)
            {
                if (m == null) continue;
                m.mainTexture = Art.SpeedLines(theme);
            }
        }

        void BuildSpeedLines()
        {
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                var go = new GameObject("SpeedLines", typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(_root, false);
                float x = side * (Layout.RoadHalf + 0.32f);
                go.transform.position = new Vector3(x, 0f, 4f);
                go.transform.localScale = new Vector3(0.5f, 46f, 1f);
                go.GetComponent<MeshFilter>().sharedMesh = QuadMesh();

                var mat = new Material(AdditiveShader) { name = "SpeedMat" };
                mat.mainTexture = Art.SpeedLines(_theme);
                mat.mainTextureScale = new Vector2(1f, 10f);
                mat.color = new Color(1f, 1f, 1f, 0.0f);
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.sortingOrder = 10;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _speedLines.Add(mr);
                _speedMats.Add(mat);
            }
        }

        void BuildDecor()
        {
            for (int side = 0; side < 2; side++)
            {
                for (int i = 0; i < DecorPerSide; i++)
                {
                    var go = new GameObject("Decor", typeof(SpriteRenderer));
                    go.transform.SetParent(_root, false);
                    var sr = go.GetComponent<SpriteRenderer>();
                    sr.sprite = Art.Decor(_theme.decorStyle);
                    sr.sortingOrder = -10;
                    sr.color = new Color(1f, 1f, 1f, 0.95f);
                    float x = (side == 0 ? -1f : 1f) * (Layout.RoadHalf + 0.95f);
                    float y = Layout.SpawnY - i * DecorSpacing;
                    go.transform.position = new Vector3(x, y, 6f);
                    _decor.Add(new Decor { sr = sr, y = y, side = side });
                }
            }
        }

        void BuildOverlays()
        {
            var fogGo = new GameObject("Fog", typeof(SpriteRenderer));
            fogGo.transform.SetParent(_root, false);
            _fog = fogGo.GetComponent<SpriteRenderer>();
            _fog.sprite = TextureSprite(Art.Fog(Layout.OrthoSize * 0.55f, _theme), 40f);
            _fog.sortingOrder = 20;
            _fog.transform.position = new Vector3(0f, Layout.OrthoSize - Layout.OrthoSize * 0.275f - 1.2f, 2f);
            Fit(_fog, Layout.ViewHalfWidth * 2f + 2f, Layout.OrthoSize * 0.55f);

            var washGo = new GameObject("ThemeWash", typeof(SpriteRenderer));
            washGo.transform.SetParent(_root, false);
            _themeWash = washGo.GetComponent<SpriteRenderer>();
            _themeWash.sprite = Art.White();
            _themeWash.color = new Color(0f, 0f, 0f, 0f);
            _themeWash.sortingOrder = 30;
            _themeWash.transform.position = new Vector3(0f, 0f, 1.5f);
            Fit(_themeWash, Layout.ViewHalfWidth * 2f + 2f, Layout.OrthoSize * 2f + 2f);
        }

        public void SetTheme(int index, bool instant = false)
        {
            index = ((index % Palette.Themes.Length) + Palette.Themes.Length) % Palette.Themes.Length;
            var next = Palette.Themes[index];
            if (instant)
            {
                _themeIndex = index;
                _theme = next;
                ApplyThemeTextures(_theme, true);
                return;
            }
            _pendingTheme = next;
            _themeIndex = index;
            _washT = 0f;
        }

        public void Tick(float dt, float speed, float speedT)
        {
            _distance += speed * dt;

            if (_roadMat != null)
                _roadMat.mainTextureOffset = new Vector2(0f, _distance / Mathf.Max(0.001f, _tileWorld));

            for (int i = 0; i < _speedMats.Count; i++)
            {
                var m = _speedMats[i];
                if (m == null) continue;
                m.mainTextureOffset = new Vector2(0f, (_distance / 5.5f) % 1f);
                float a = Mathf.Lerp(0f, 0.55f, Mathf.Clamp01(speedT));
                var c = _theme.laneLine;
                m.color = new Color(c.r, c.g, c.b, a);
            }

            for (int i = 0; i < _decor.Count; i++)
            {
                var d = _decor[i];
                d.y -= speed * dt;
                if (d.y < Layout.DespawnY - 2f) d.y += DecorPerSide * DecorSpacing;
                d.sr.transform.localPosition = new Vector3(d.sr.transform.localPosition.x, d.y, 6f);
            }

            if (_stars != null) _stars.color = new Color(1f, 1f, 1f, _theme.starAlpha);

            if (_washT >= 0f)
            {
                _washT += dt;
                const float half = 0.32f;
                if (_washT < half)
                {
                    float k = _washT / half;
                    _themeWash.color = new Color(_pendingTheme.skyBottom.r, _pendingTheme.skyBottom.g,
                                                 _pendingTheme.skyBottom.b, k * 0.9f);
                }
                else
                {
                    if (_theme.id != _pendingTheme.id)
                    {
                        _theme = _pendingTheme;
                        ApplyThemeTextures(_theme, true);
                    }
                    float k = Mathf.Clamp01((_washT - half) / 0.45f);
                    _themeWash.color = new Color(_pendingTheme.skyBottom.r, _pendingTheme.skyBottom.g,
                                                 _pendingTheme.skyBottom.b, (1f - k) * 0.9f);
                    if (k >= 1f) _washT = -1f;
                }
            }
        }

        public void ResetForRun()
        {
            _distance = 0f;
            SetTheme(0, true);
            if (_themeWash != null) _themeWash.color = new Color(0f, 0f, 0f, 0f);
        }
    }
}


