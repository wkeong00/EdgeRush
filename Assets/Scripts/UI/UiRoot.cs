using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EdgeRush
{
    /// <summary>Owns the canvases, the screens and the small transition coroutines.</summary>
    public class UiRoot : MonoBehaviour
    {
        public static UiRoot I { get; private set; }

        public Canvas Canvas;
        public RectTransform RootRect;
        public RectTransform Safe;

        public HudScreen Hud;
        public MainMenuScreen Menu;
        public PauseScreen Pause;
        public ReviveScreen Revive;
        public ResultsScreen Results;
        public GarageScreen Garage;
        public DailyScreen Daily;
        public QuestsScreen Quests;
        public SettingsScreen Settings;
        public CountdownView Countdown;
        public ToastView Toast;
        public FlashLayer Flash;

        public void Build()
        {
            I = this;
            Ui.InitFont();

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            Canvas = canvasGo.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            canvasGo.AddComponent<GraphicRaycaster>();
            RootRect = (RectTransform)canvasGo.transform;

            // Event system
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }

            // Vignette (below everything)
            var vig = Ui.AddImage(RootRect, "Vignette", Art.FromTexture(Art.Vignette()), Color.white, false);
            Ui.Stretch(vig.rectTransform);

            // Safe area container for all screens
            var safeGo = Ui.Rect("Safe", RootRect);
            Safe = Ui.Stretch(safeGo);
            Safe.gameObject.AddComponent<SafeAreaFitter>();

            Hud = new HudScreen();
            Menu = new MainMenuScreen();
            Pause = new PauseScreen();
            Revive = new ReviveScreen();
            Results = new ResultsScreen();
            Garage = new GarageScreen();
            Daily = new DailyScreen();
            Quests = new QuestsScreen();
            Settings = new SettingsScreen();
            Countdown = new CountdownView();
            Toast = new ToastView();

            Hud.Build(this);
            Menu.Build(this);
            Pause.Build(this);
            Revive.Build(this);
            Results.Build(this);
            Garage.Build(this);
            Daily.Build(this);
            Quests.Build(this);
            Settings.Build(this);
            Countdown.Build(this);
            Toast.Build(this);

            Flash = new FlashLayer();
            Flash.Build(this);
        }

        public RectTransform NewScreenRoot(string name)
        {
            var rt = Ui.Rect(name, Safe);
            Ui.Stretch(rt);
            return rt;
        }

        // ------------------------------------------------------------ transitions
        public void FadeIn(UiScreen s)
        {
            if (s.Root == null) return;
            s.Root.gameObject.SetActive(true);
            s.Root.SetAsLastSibling();
            s.Group.alpha = 0f;
            s.Group.blocksRaycasts = true;
            if (s.FadeRoutine != null) StopCoroutine(s.FadeRoutine);
            s.FadeRoutine = StartCoroutine(FadeRoutine(s, 0f, 1f, 0.16f));
        }

        public void FadeOut(UiScreen s)
        {
            if (s.Root == null || !s.Root.gameObject.activeSelf) return;
            s.Group.blocksRaycasts = false;
            if (s.FadeRoutine != null) StopCoroutine(s.FadeRoutine);
            s.FadeRoutine = StartCoroutine(FadeRoutine(s, s.Group.alpha, 0f, 0.13f));
        }

        IEnumerator FadeRoutine(UiScreen s, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                s.Group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            s.Group.alpha = to;
            if (to <= 0f) s.Root.gameObject.SetActive(false);
        }

        /// <summary>Small score popup that floats up and fades out.</summary>
        public void SpawnFloating(string text, Vector2 pos, Color color, int size = 46)
        {
            var t = Ui.MakeLabel(Safe, text, size, color, TextAnchor.MiddleCenter, pos.x, pos.y, 420, 90);
            StartCoroutine(FloatAndDestroy(t));
        }

        IEnumerator FloatAndDestroy(Text t)
        {
            float duration = 0.85f;
            var rt = t.rectTransform;
            var start = rt.anchoredPosition;
            var c = t.color;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(elapsed / duration);
                rt.anchoredPosition = start + new Vector2(0f, 90f * k);
                t.color = new Color(c.r, c.g, c.b, 1f - k * k);
                yield return null;
            }
            Destroy(t.gameObject);
        }

        public void Pop(Transform target, float from = 0.92f, float duration = 0.18f)
        {
            StartCoroutine(PopRoutine(target, from, duration));
        }

        IEnumerator PopRoutine(Transform target, float from, float duration)
        {
            if (target == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                float s = Mathf.Lerp(from, 1f, 1f - Mathf.Pow(1f - k, 3f));
                target.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            target.localScale = Vector3.one;
        }

        public void FloatText(Text label, float rise, float duration)
        {
            StartCoroutine(FloatTextRoutine(label, rise, duration));
        }

        IEnumerator FloatTextRoutine(Text label, float rise, float duration)
        {
            var rt = label.rectTransform;
            var start = rt.anchoredPosition;
            var c = label.color;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                rt.anchoredPosition = start + new Vector2(0f, rise * k);
                label.color = new Color(c.r, c.g, c.b, 1f - k);
                yield return null;
            }
            rt.anchoredPosition = start;
            label.color = new Color(c.r, c.g, c.b, 0f);
        }

        public void CloseAllMenus()
        {
            Menu.Hide(true);
            Garage.Hide(true);
            Daily.Hide(true);
            Quests.Hide(true);
            Settings.Hide(true);
            Pause.Hide(true);
            Results.Hide(true);
            Revive.Hide(true);
            Hud.Hide(true);
            Countdown.Hide();
        }
    }

    /// <summary>Base class for a full screen UI panel.</summary>
    public abstract class UiScreen
    {
        protected UiRoot Hub;
        public RectTransform Root;
        public CanvasGroup Group { get; private set; }
        public bool Visible { get; private set; }
        public Coroutine FadeRoutine;

        public void Build(UiRoot ui)
        {
            Hub = ui;
            Root = ui.NewScreenRoot(GetType().Name);
            Group = Ui.Group(Root.gameObject);
            Group.alpha = 0f;
            Construct();
            Root.gameObject.SetActive(false);
        }

        protected abstract void Construct();

        public virtual void Refresh() { }

        public virtual void Show(bool instant = false)
        {
            Visible = true;
            Refresh();
            if (instant)
            {
                Group.alpha = 1f;
                Root.gameObject.SetActive(true);
                Root.SetAsLastSibling();
                return;
            }
            Hub.FadeIn(this);
        }

        public virtual void Hide(bool instant = false)
        {
            Visible = false;
            if (Root == null) return;
            if (instant)
            {
                Group.alpha = 0f;
                Root.gameObject.SetActive(false);
                return;
            }
            Hub.FadeOut(this);
        }

        public void SetTop()
        {
            if (Root != null) Root.SetAsLastSibling();
        }
    }

    /// <summary>White/coloured full screen flash used for impacts.</summary>
    public class FlashLayer
    {
        UiRoot _ui;
        Image _img;
        Coroutine _routine;

        public void Build(UiRoot ui)
        {
            _ui = ui;
            _img = Ui.AddImage(ui.RootRect, "Flash", Art.White(), new Color(1f, 1f, 1f, 0f), false);
            Ui.Stretch(_img.rectTransform);
            _img.transform.SetAsLastSibling();
            _img.raycastTarget = false;
        }

        public void Flash(Color color, float duration = 0.35f, float peak = 0.8f)
        {
            if (_img == null) return;
            if (_routine != null) _ui.StopCoroutine(_routine);
            _routine = _ui.StartCoroutine(Run(color, duration, peak));
            _img.transform.SetAsLastSibling();
        }

        IEnumerator Run(Color color, float duration, float peak)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / duration;
                var c = color;
                c.a = peak * (1f - k) * (1f - k);
                _img.color = c;
                yield return null;
            }
            _img.color = new Color(0f, 0f, 0f, 0f);
        }
    }

    /// <summary>Transient message at the top of the screen.</summary>
    public class ToastView
    {
        UiRoot _ui;
        RectTransform _root;
        CanvasGroup _group;
        Text _text;
        Coroutine _routine;

        public void Build(UiRoot ui)
        {
            _ui = ui;
            var panel = Ui.MakePanel(ui.Safe, "Toast", 700, 96, new Color(0.06f, 0.08f, 0.16f, 0.95f), Palette.Cyan, 0.9f);
            _root = panel;
            Ui.Place(_root, 0, 690, 700, 96);
            _group = Ui.Group(_root.gameObject);
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _text = Ui.MakeLabel(_root, "", 36, Palette.TextMain, TextAnchor.MiddleCenter, 0, 0, 640, 80);
            _root.gameObject.SetActive(false);
        }

        public void Show(string message, Color? color = null)
        {
            if (_root == null) return;
            _text.text = message;
            _text.color = color ?? Palette.TextMain;
            if (_routine != null) _ui.StopCoroutine(_routine);
            _routine = _ui.StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            _root.gameObject.SetActive(true);
            _root.SetAsLastSibling();
            float t = 0f;
            while (t < 0.18f)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Clamp01(t / 0.18f);
                yield return null;
            }
            _group.alpha = 1f;
            yield return new WaitForSecondsRealtime(1.5f);
            t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = 1f - Mathf.Clamp01(t / 0.3f);
                yield return null;
            }
            _root.gameObject.SetActive(false);
        }
    }

    /// <summary>3-2-1 countdown in the middle of the screen.</summary>
    public class CountdownView
    {
        UiRoot _ui;
        RectTransform _root;
        CanvasGroup _group;
        Text _text;
        Text _sub;

        public void Build(UiRoot ui)
        {
            _ui = ui;
            _root = Ui.Rect("Countdown", ui.Safe);
            Ui.Stretch(_root);
            _group = Ui.Group(_root.gameObject);
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _text = Ui.MakeLabel(_root, "3", 220, Palette.Cyan, TextAnchor.MiddleCenter, 0, 60, 900, 300);
            Ui.AddOutline(_text, new Color(0f, 0.1f, 0.2f, 0.9f), 6f);
            _sub = Ui.MakeLabel(_root, "", 40, Palette.TextDim, TextAnchor.MiddleCenter, 0, -120, 900, 60);
            _root.gameObject.SetActive(false);
        }

        public void Show(string text, string sub = "")
        {
            _root.gameObject.SetActive(true);
            _root.SetAsLastSibling();
            _group.alpha = 1f;
            _text.text = text;
            _sub.text = sub;
            _ui.Pop(_text.transform, 1.5f, 0.22f);
        }

        public void Hide()
        {
            if (_root == null) return;
            _root.gameObject.SetActive(false);
            _group.alpha = 0f;
        }
    }
}



