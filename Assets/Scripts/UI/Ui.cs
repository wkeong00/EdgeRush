using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EdgeRush
{
    /// <summary>Small helpers for building the whole UI from code (no prefabs, no scene setup).</summary>
    public static class Ui
    {
        public static Font Font { get; private set; }

        public static void InitFont()
        {
            if (Font != null) return;
            try { Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (Font == null)
            {
                try { Font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }
            if (Font == null) Font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            if (Font == null) Debug.LogError("[Ui] No font available - text will not render.");
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static RectTransform Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, y);
            return rt;
        }

        /// <summary>Positions an element from an edge of its safe-area parent.</summary>
        public static RectTransform Anchor(RectTransform rt, Vector2 anchor, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, y);
            return rt;
        }

        public static RectTransform Top(RectTransform rt, float inset, float w, float h, float x = 0)
            => Anchor(rt, new Vector2(0.5f, 1f), x, -inset - h * 0.5f, w, h);

        public static RectTransform Bottom(RectTransform rt, float inset, float w, float h, float x = 0)
            => Anchor(rt, new Vector2(0.5f, 0f), x, inset + h * 0.5f, w, h);

        public static RectTransform FitPanel(Transform parent, string name, float w, float h, Color edge)
        {
            var rt = MakePanel(parent, name, w, h, Palette.PanelFill, edge, 0.9f);
            Place(rt, 0, 0, w, h);
            rt.gameObject.AddComponent<PanelSizeFitter>().Configure(w, h, 40f);
            return rt;
        }

        /// <summary>One wallet component shared by menu, garage and missions.</summary>
        public static Text Wallet(Transform parent, out RectTransform root)
        {
            root = MakePanel(parent, "Wallet", 300, 84, new Color(0.05f, 0.07f, 0.13f, 0.85f), Palette.Gold, 0.55f);
            root.GetComponent<Image>().raycastTarget = false;
            var icon = AddImage(root, "Coin", Art.Coin(), Color.white);
            Place(icon.rectTransform, -106, 0, 52, 52);
            var value = MakeLabel(root, "0", 38, Palette.Gold, TextAnchor.MiddleLeft, 30, 0, 210, 64);
            value.resizeTextForBestFit = true;
            value.resizeTextMinSize = 24;
            value.resizeTextMaxSize = 38;
            return value;
        }

        /// <summary>Viewport consumes drags, while fixed headers and footers stay in place.</summary>
        public static RectTransform VerticalScroll(Transform parent, string name, float left, float bottom, float right, float top)
        {
            var root = Rect(name, parent);
            Stretch(root, left, bottom, right, top);
            var scroll = root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            var viewportImage = AddFill(root, "Viewport", new Color(1f, 1f, 1f, 0.01f), true);
            Stretch(viewportImage.rectTransform);
            var mask = viewportImage.gameObject.AddComponent<UnityEngine.UI.Mask>();
            mask.showMaskGraphic = false;
            var content = Rect("Content", viewportImage.transform);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = 24;
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewportImage.rectTransform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 45f;
            return content;
        }

        public static Image AddImage(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null && sprite.border.sqrMagnitude > 0f) img.type = Image.Type.Sliced;
            return img;
        }

        public static Image AddFill(Transform parent, string name, Color color, bool raycast = false)
            => AddImage(parent, name, Art.White(), color, raycast);

        /// <summary>Rounded panel with a neon rim. Returns the panel's RectTransform.</summary>
        public static RectTransform MakePanel(Transform parent, string name, float w, float h, Color fill, Color edge, float edgeAlpha = 1f)
        {
            var rt = Rect(name, parent);
            rt.sizeDelta = new Vector2(w, h);

            var bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = Art.PanelFill();
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 1f;
            bg.color = fill;
            bg.raycastTarget = true;

            if (edgeAlpha > 0f)
            {
                var rim = AddImage(rt, "Rim", Art.PanelRing(), color: edge);
                rim.type = Image.Type.Sliced;
                Stretch(rim.rectTransform, 0, 0, 0, 0);
                rim.color = new Color(edge.r, edge.g, edge.b, edge.a * edgeAlpha);
            }
            return rt;
        }

        public static Text MakeLabel(Transform parent, string text, int size, Color color, TextAnchor anchor,
                                     float x, float y, float w, float h, bool bold = true)
        {
            InitFont();
            var rt = Rect("Label", parent);
            Place(rt, x, y, w, h);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.raycastTarget = false;
            return t;
        }

        public static Text MakeLabel(Transform parent, string text, int size, Color color, TextAnchor anchor,
                                     float x, float y, float w, float h, bool bold, bool wrap)
        {
            var t = MakeLabel(parent, text, size, color, anchor, x, y, w, h, bold);
            if (wrap)
            {
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Truncate;
            }
            return t;
        }

        public static Button MakeButton(Transform parent, string label, int fontSize, Color fill, Color textColor,
                                        UnityAction onClick, float x, float y, float w, float h, float edgeAlpha = 0.8f,
                                        Color? edge = null, Sprite icon = null)
        {
            var rt = MakePanel(parent, "Button_" + label, w, h, fill, edge ?? Palette.Cyan, edgeAlpha);
            Place(rt, x, y, w, h);

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = rt.GetComponent<Image>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.06f;
            btn.colors = colors;
            btn.onClick.AddListener(() =>
            {
                if (AudioDirector.I != null) AudioDirector.I.PlayUiClick();
                Haptics.Light();
                onClick?.Invoke();
            });
            rt.gameObject.AddComponent<ButtonFx>();

            float textOffset = 0f;
            if (icon != null)
            {
                var img = AddImage(rt, "Icon", icon, textColor);
                Place(img.rectTransform, -w * 0.5f + h * 0.55f, 0, h * 0.5f, h * 0.5f);
                textOffset = h * 0.22f;
            }
            var t = MakeLabel(rt, label, fontSize, textColor, TextAnchor.MiddleCenter, textOffset, 0, w - h * 0.6f, h * 0.9f);
            return btn;
        }

        public static Button MakeIconButton(Transform parent, string name, Sprite icon, Color tint, UnityAction onClick,
                                            float x, float y, float size, Color? bg = null)
        {
            var rt = Rect(name, parent);
            Place(rt, x, y, size, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = Art.CircleSolid();
            img.type = Image.Type.Simple;
            img.color = bg ?? new Color(0.07f, 0.09f, 0.16f, 0.78f);
            img.raycastTarget = true;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.pressedColor = new Color(0.75f, 0.75f, 0.8f, 1f);
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() =>
            {
                if (AudioDirector.I != null) AudioDirector.I.PlayUiClick();
                Haptics.Light();
                onClick?.Invoke();
            });
            rt.gameObject.AddComponent<ButtonFx>();

            var ic = AddImage(rt, "Glyph", icon, tint);
            Place(ic.rectTransform, 0, 0, size * 0.52f, size * 0.52f);
            return btn;
        }

        public static void AddShadow(Text t, float alpha = 0.55f)
        {
            var s = t.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0, 0, 0, alpha);
            s.effectDistance = new Vector2(3f, -3f);
        }

        public static void AddOutline(Text t, Color color, float thickness = 2f)
        {
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = color;
            o.effectDistance = new Vector2(thickness, -thickness);
        }

        /// <summary>Horizontal progress bar with a rounded track and fill.</summary>
        public static Image MakeBar(Transform parent, float x, float y, float w, float h, Color track, Color fill)
        {
            var bg = AddImage(parent, "BarBg", Art.Pill(), track);
            Place(bg.rectTransform, x, y, w, h);
            var fg = AddImage(bg.rectTransform, "BarFill", Art.Pill(), fill);
            var frt = fg.rectTransform;
            frt.anchorMin = new Vector2(0, 0);
            frt.anchorMax = new Vector2(1, 1);
            frt.offsetMin = new Vector2(3, 3);
            frt.offsetMax = new Vector2(-3, -3);
            fg.sprite = Art.Pill();
            fg.type = Image.Type.Sliced;
            fg.pixelsPerUnitMultiplier = 1f;

            // Use a mask-free approach: scale the fill horizontally around its left edge.
            frt.pivot = new Vector2(0f, 0.5f);
            frt.anchorMin = new Vector2(0, 0);
            frt.anchorMax = new Vector2(1, 1);
            return fg;
        }

        public static void SetBar(Image fill, float t)
        {
            if (fill == null) return;
            var rt = fill.rectTransform;
            t = Mathf.Clamp01(t);
            rt.anchorMax = new Vector2(Mathf.Max(0.001f, t), 1f);
            rt.offsetMax = new Vector2(-3f, -3f);
            fill.enabled = t > 0.001f;
        }

        public static CanvasGroup Group(GameObject go)
        {
            var g = go.GetComponent<CanvasGroup>();
            if (g == null) g = go.AddComponent<CanvasGroup>();
            return g;
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }
    }

    /// <summary>Squash/scale feedback for buttons.</summary>
    public class ButtonFx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        Vector3 _base = Vector3.one;
        bool _pressed;

        void OnDisable()
        {
            _pressed = false;
            transform.localScale = _base;
        }

        void Awake() { _base = transform.localScale; }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            transform.localScale = _base * 0.94f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pressed) return;
            _pressed = false;
            transform.localScale = _base;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_pressed) return;
            _pressed = false;
            transform.localScale = _base;
        }
    }

    /// <summary>Caps a modal within the single safe-area container; never applies another inset.</summary>
    public class PanelSizeFitter : MonoBehaviour
    {
        RectTransform _rt;
        float _width, _height, _margin;
        public void Configure(float width, float height, float margin)
        {
            _rt = (RectTransform)transform;
            _width = width;
            _height = height;
            _margin = margin;
            Apply();
        }
        void OnEnable() { Apply(); }
        void OnRectTransformDimensionsChange() { Apply(); }
        void Apply()
        {
            if (_rt == null || _width <= 0 || !(_rt.parent is RectTransform)) return;
            var parent = (RectTransform)_rt.parent;
            if (parent.rect.width <= 0 || parent.rect.height <= 0) return;
            var size = new Vector2(Mathf.Min(_width, Mathf.Max(1, parent.rect.width - _margin * 2)),
                                   Mathf.Min(_height, Mathf.Max(1, parent.rect.height - _margin * 2)));
            if ((_rt.sizeDelta - size).sqrMagnitude > 0.01f) _rt.sizeDelta = size;
        }
    }

    /// <summary>Keeps a full screen rect inside the device's safe area (notches, rounded corners).</summary>
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform _rt;
        Rect _last;
        Vector2Int _lastScreen;

        void Awake()
        {
            _rt = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != _last || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height) Apply();
        }

        void Apply()
        {
            var safe = Screen.safeArea;
            _last = safe;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var min = safe.position;
            var max = safe.position + safe.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;
            _rt.anchorMin = min;
            _rt.anchorMax = max;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}

