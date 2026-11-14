using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeRush
{
    public enum AdPlacement { Revive, DoubleCoins, DailyBonus, SkinUnlock }

    /// <summary>Release ads require a real provider and production IDs. Disable ads explicitly for a free build.</summary>
    public static class AdConfiguration
    {
        public static bool AdsEnabled = true;
        public static bool DailyBonusEnabled = true;
        public static bool SkinUnlockEnabled = true;
        public const float MinimumIntervalSeconds = 60f;
        public const int NewPlayerCompletedRuns = 3;
        public static readonly Dictionary<AdPlacement, string> UnitIds = new Dictionary<AdPlacement, string>
        {
            { AdPlacement.Revive, "" }, { AdPlacement.DoubleCoins, "" },
            { AdPlacement.DailyBonus, "" }, { AdPlacement.SkinUnlock, "" },
        };
        public static bool PlacementEnabled(AdPlacement p) => AdsEnabled &&
            (p != AdPlacement.DailyBonus || DailyBonusEnabled) && (p != AdPlacement.SkinUnlock || SkinUnlockEnabled);
        public static bool IsProductionUnitId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !id.StartsWith("ca-app-pub-", StringComparison.Ordinal) ||
                id.Contains("3940256099942544")) return false;
            var parts = id.Substring("ca-app-pub-".Length).Split('/');
            return parts.Length == 2 && parts[0].Length == 16 && parts[1].Length == 10 &&
                   AllDigits(parts[0]) && AllDigits(parts[1]);
        }
        static bool AllDigits(string value)
        {
            for (int i = 0; i < value.Length; i++) if (value[i] < '0' || value[i] > '9') return false;
            return true;
        }
        public static bool IsConfigured(bool allowTestIds = false)
        {
            foreach (var p in AdManager.AllPlacements)
            {
                if (!PlacementEnabled(p)) continue;
                string id;
                if (!UnitIds.TryGetValue(p, out id) || string.IsNullOrWhiteSpace(id)) return false;
                if (!allowTestIds && !IsProductionUnitId(id)) return false;
            }
            return true;
        }
    }

    public sealed class AdCallbacks
    {
        public Action Opened, Impression, Reward, Closed;
        public Action<string> Failed;
        public Action<long, string, string> Paid;
    }
    public interface IAdProvider : IDisposable
    {
        string Name { get; }
        void Initialize(Action<bool, string> completed);
        bool IsReady(AdPlacement placement);
        void Load(AdPlacement placement, Action<bool, string> completed);
        void Clear(AdPlacement placement);
        void Show(AdPlacement placement, AdCallbacks callbacks);
    }
    /// <summary>Reward and terminal callbacks are independently idempotent. Failure cannot revoke a reward.</summary>
    public sealed class AdCompletionGuard
    {
        public bool Rewarded { get; private set; }
        public bool Closed { get; private set; }
        public bool TryReward() { if (Rewarded || Closed) return false; Rewarded = true; return true; }
        public bool TryClose() { if (Closed) return false; Closed = true; return true; }
    }

    public class AdManager : MonoBehaviour
    {
        sealed class LoadState
        {
            public bool pending;
            public int generation, attempts;
            public float started, retryAt;
        }
        sealed class ShowState
        {
            public string id;
            public AdPlacement placement;
            public float started;
            public bool opened, impression;
            public readonly AdCompletionGuard guard = new AdCompletionGuard();
            public Action<bool> closed;
            public Action<string> reward;
        }
        const int MaxAttempts = 4;
        const float RequestTimeoutSeconds = 20f;
        readonly Dictionary<AdPlacement, LoadState> _loads = new Dictionary<AdPlacement, LoadState>();
        readonly ConcurrentQueue<Action> _mainThread = new ConcurrentQueue<Action>();
        IAdProvider _provider;
        ShowState _show;
        int _threadId, _initGeneration, _initAttempts;
        bool _initializing, _disposed, _focused = true, _paused;
        bool _runEligibilityCaptured, _runAdsEligible;
        float _initStarted, _initRetryAt, _lastShownAt = -999f;
        string _configurationError;
        GameObject _recoveryRoot;
        public static AdManager I { get; private set; }
        public string ProviderName => _provider != null ? _provider.Name : "none";
        public bool Initialized { get; private set; }
        public bool IsShowing => _show != null;
        public bool RunAdsEligible => _runEligibilityCaptured ? _runAdsEligible :
            GameApp.I == null || GameApp.I.Save.completedRuns >= AdConfiguration.NewPlayerCompletedRuns;
        public float SecondsSinceLastAd => Time.unscaledTime - _lastShownAt;
        public static readonly AdPlacement[] AllPlacements =
        {
            AdPlacement.Revive, AdPlacement.DoubleCoins, AdPlacement.DailyBonus, AdPlacement.SkinUnlock,
        };
        public void Init()
        {
            if (_provider != null || _disposed) return;
            I = this;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            foreach (var p in AllPlacements) _loads[p] = new LoadState();
            _provider = CreateProvider();
            if (_provider != null) InitializeProvider();
        }
        IAdProvider CreateProvider()
        {
            if (!AdConfiguration.AdsEnabled) { _configurationError = "ADS DISABLED"; return null; }
#if EDGERUSH_ADMOB && (UNITY_ANDROID || UNITY_IOS)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (AdConfiguration.IsConfigured(true)) return new AdMobProvider();
#else
            if (AdConfiguration.IsConfigured()) return new AdMobProvider();
#endif
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return new MockAdProvider();
#else
            _configurationError = "ADS NOT CONFIGURED";
            Debug.LogError("[Ads] No production provider configured. Ads disabled; Mock is never used in release.");
            return null;
#endif
        }
        void Dispatch(Action action)
        {
            if (_disposed) return;
            if (Thread.CurrentThread.ManagedThreadId == _threadId) action();
            else _mainThread.Enqueue(action);
        }
        void InitializeProvider()
        {
            if (_provider == null || Initialized || _initializing || _initAttempts >= MaxAttempts) return;
            _initializing = true;
            _initAttempts++;
            _initStarted = Time.unscaledTime;
            int generation = ++_initGeneration;
            try
            {
                _provider.Initialize((ok, error) => Dispatch(() =>
                {
                    if (!_initializing || generation != _initGeneration) return;
                    _initializing = false;
                    Initialized = ok;
                    if (ok) LoadAll();
                    else { _initRetryAt = Time.unscaledTime + RetryDelay(_initAttempts); Analytics.Event("ad_init_failed", error); }
                }));
            }
            catch (Exception e)
            {
                _initializing = false;
                _initRetryAt = Time.unscaledTime + RetryDelay(_initAttempts);
                Analytics.Event("ad_init_failed", e.GetType().Name);
            }
        }
        static float RetryDelay(int attempt) => Mathf.Min(30f, 2f * Mathf.Pow(2f, attempt - 1));
        public void LoadAll()
        {
            if (!Initialized || _provider == null) return;
            foreach (var p in AllPlacements) RequestLoad(p);
        }
        void RequestLoad(AdPlacement placement)
        {
            if (!Initialized || !AdConfiguration.PlacementEnabled(placement)) return;
            var state = _loads[placement];
            if (state.pending || state.attempts >= MaxAttempts || Time.unscaledTime < state.retryAt ||
                (_show != null && _show.placement == placement) || _provider.IsReady(placement)) return;
            state.pending = true;
            state.attempts++;
            state.started = Time.unscaledTime;
            int generation = ++state.generation;
            try
            {
                _provider.Load(placement, (ok, error) => Dispatch(() =>
                {
                    if (!state.pending || generation != state.generation) return;
                    state.pending = false;
                    if (ok) { state.attempts = 0; state.retryAt = 0; }
                    else { state.retryAt = Time.unscaledTime + RetryDelay(state.attempts); Analytics.Event("ad_load_failed", placement + " reason=" + error); }
                }));
            }
            catch (Exception e)
            {
                state.pending = false;
                state.retryAt = Time.unscaledTime + RetryDelay(state.attempts);
                Analytics.Event("ad_load_failed", placement + " reason=" + e.GetType().Name);
            }
        }
        public string UnavailableReason(AdPlacement placement)
        {
            if (!AdConfiguration.PlacementEnabled(placement)) return "ADS DISABLED";
            if (_show != null) return "AD IN PROGRESS";
            if ((placement == AdPlacement.Revive || placement == AdPlacement.DoubleCoins) && !RunAdsEligible)
                return "COMPLETE 3 RUNS FIRST";
            if (SecondsSinceLastAd < AdConfiguration.MinimumIntervalSeconds)
                return "NEXT AD IN " + Mathf.CeilToInt(AdConfiguration.MinimumIntervalSeconds - SecondsSinceLastAd) + "s";
            if (_provider == null) return _configurationError ?? "ADS NOT CONFIGURED";
            if (!Initialized) return _initAttempts >= MaxAttempts ? "AD INITIALIZATION FAILED" : "CONNECTING ADS";
            if (!_provider.IsReady(placement)) return _loads[placement].attempts >= MaxAttempts ? "AD UNAVAILABLE" : "LOADING AD";
            return null;
        }
        public bool Available(AdPlacement placement) => UnavailableReason(placement) == null;
        /// <summary>Capture before a formal run; its third result must remain ad-free after completedRuns increments.</summary>
        public void BeginRun()
        {
            _runEligibilityCaptured = true;
            _runAdsEligible = GameApp.I != null && GameApp.I.Save.completedRuns >= AdConfiguration.NewPlayerCompletedRuns;
        }
        public bool AnyReady { get { foreach (var p in AllPlacements) if (Available(p)) return true; return false; } }

        /// <summary>onReward persists eligibility immediately; onClosed restores UI/gameplay exactly once.</summary>
        public void Show(AdPlacement placement, Action<bool> onClosed, Action<string> onReward = null)
        {
            Analytics.Event("ad_click", placement.ToString());
            string reason = UnavailableReason(placement);
            if (reason != null)
            {
                Analytics.Event("ad_unavailable", placement + " reason=" + reason);
                SafeInvoke(() => onClosed?.Invoke(false));
                return;
            }
            var show = new ShowState { id = Guid.NewGuid().ToString("N"), placement = placement,
                started = Time.unscaledTime, closed = onClosed, reward = onReward };
            _show = show;
            Analytics.Event("ad_request", Detail(show));
            if (AudioDirector.I != null) AudioDirector.I.DuckForAd(true);
            try
            {
                _provider.Show(placement, new AdCallbacks
                {
                    Opened = () => Dispatch(() =>
                    {
                        if (!Current(show) || show.opened) return;
                        show.opened = true;
                        _lastShownAt = Time.unscaledTime;
                        Analytics.Event("ad_opened", Detail(show));
                    }),
                    Impression = () => Dispatch(() =>
                    {
                        if (!Current(show) || show.impression) return;
                        show.impression = true;
                        Analytics.Event("ad_impression", Detail(show));
                    }),
                    Reward = () => Dispatch(() => Reward(show)),
                    Closed = () => Dispatch(() => Close(show, null)),
                    Failed = error => Dispatch(() => Close(show, error ?? "show_failed")),
                    Paid = (micros, currency, precision) => Dispatch(() =>
                    {
                        if (!_disposed) Analytics.Event("ad_paid", Detail(show) + " micros=" + micros + " currency=" + currency + " precision=" + precision);
                    }),
                });
            }
            catch (Exception e) { Close(show, e.GetType().Name); }
        }
        bool Current(ShowState show) => !_disposed && ReferenceEquals(_show, show) && !show.guard.Closed;
        static string Detail(ShowState show) => "placement=" + show.placement + " showId=" + show.id;
        void Reward(ShowState show)
        {
            if (!Current(show) || !show.guard.TryReward()) return;
            Analytics.Event("ad_reward", Detail(show));
            if (GameApp.I != null) GameApp.I.Save.totalAdViews++;
            SafeInvoke(() => show.reward?.Invoke(show.id));
            if (GameApp.I != null) GameApp.I.SaveNow();
        }
        void Close(ShowState show, string error)
        {
            if (!Current(show) || !show.guard.TryClose()) return;
            _show = null;
            if (_recoveryRoot != null) _recoveryRoot.SetActive(false);
            if (AudioDirector.I != null) AudioDirector.I.DuckForAd(false);
            Analytics.Event(error == null ? "ad_closed" : "ad_failed", Detail(show) + " rewarded=" + show.guard.Rewarded +
                (error == null ? "" : " reason=" + error));
            var state = _loads[show.placement];
            state.attempts = 0;
            state.retryAt = Time.unscaledTime + 1f;
            SafeInvoke(() => show.closed?.Invoke(show.guard.Rewarded));
        }
        static void SafeInvoke(Action callback)
        {
            try { callback?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
        }
        void Update()
        {
            Action callback;
            while (_mainThread.TryDequeue(out callback)) SafeInvoke(callback);
            if (_provider == null || _disposed) return;
            if (_initializing && Time.unscaledTime - _initStarted > RequestTimeoutSeconds)
            {
                _initializing = false;
                _initGeneration++;
                _initRetryAt = Time.unscaledTime + RetryDelay(_initAttempts);
                Analytics.Event("ad_init_failed", "timeout");
            }
            if (!Initialized && !_initializing && Time.unscaledTime >= _initRetryAt) InitializeProvider();
            if (Initialized)
            {
                foreach (var p in AllPlacements)
                {
                    var state = _loads[p];
                    if (state.pending && Time.unscaledTime - state.started > RequestTimeoutSeconds)
                    {
                        state.pending = false;
                        state.generation++;
                        _provider.Clear(p);
                        state.retryAt = Time.unscaledTime + RetryDelay(state.attempts);
                        Analytics.Event("ad_load_failed", p + " reason=timeout");
                    }
                    RequestLoad(p);
                }
            }
            // Never auto-resume while native ad content may still be visible. This action can
            // only be pressed after the player has actually returned to Unity's UI.
            if (_show != null && _focused && !_paused && Time.unscaledTime - _show.started > 90f) ShowRecoveryAction();
        }
        void ShowRecoveryAction()
        {
            if (_recoveryRoot != null) { _recoveryRoot.SetActive(true); return; }
            _recoveryRoot = new GameObject("AdReturnRecovery", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _recoveryRoot.transform.SetParent(transform, false);
            var canvas = _recoveryRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 600;
            var scaler = _recoveryRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f;
            var panel = Ui.MakePanel(_recoveryRoot.transform, "ReturnPanel", 900, 300, Palette.PanelFill, Palette.Violet);
            Ui.MakeLabel(panel, "AD RESPONSE DELAYED", 40, Palette.TextMain, TextAnchor.MiddleCenter, 0, 75, 840, 70);
            Ui.MakeButton(panel, "RETURN TO GAME", 38, Palette.PanelFillSoft, Palette.Cyan, RecoverAfterReturn, 0, -55, 700, 105);
        }
        public void RecoverAfterReturn()
        {
            if (!_focused || _paused || _show == null) return;
            Close(_show, "missing_close_user_return");
        }
        void OnApplicationFocus(bool focused) { _focused = focused; }
        void OnApplicationPause(bool paused) { _paused = paused; }
        void OnDestroy()
        {
            _disposed = true;
            _initGeneration++;
            _show = null;
            if (AudioDirector.I != null) AudioDirector.I.DuckForAd(false);
            _provider?.Dispose();
            if (I == this) I = null;
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public enum MockAdOutcome { Success, Skipped, Closed, Failed }
    /// <summary>Set NextOutcome/DelaySeconds for deterministic QA. Never compiled into a release player.</summary>
    public class MockAdProvider : IAdProvider
    {
        public static MockAdOutcome NextOutcome = MockAdOutcome.Success;
        public static float DelaySeconds = 3f;
        readonly HashSet<AdPlacement> _ready = new HashSet<AdPlacement>();
        public string Name => "MockRewarded";
        public void Initialize(Action<bool, string> completed) { completed(true, null); }
        public bool IsReady(AdPlacement placement) => _ready.Contains(placement);
        public void Load(AdPlacement placement, Action<bool, string> completed) { _ready.Add(placement); completed(true, null); }
        public void Clear(AdPlacement placement) { _ready.Remove(placement); }
        public void Show(AdPlacement placement, AdCallbacks callbacks)
        {
            _ready.Remove(placement);
            if (MockAdScreen.Instance == null)
            {
                var go = new GameObject("MockAdScreen");
                UnityEngine.Object.DontDestroyOnLoad(go);
                MockAdScreen.Instance = go.AddComponent<MockAdScreen>();
            }
            var outcome = NextOutcome;
            NextOutcome = MockAdOutcome.Success;
            MockAdScreen.Instance.Play(placement, callbacks, outcome, DelaySeconds);
        }
        public void Dispose() { _ready.Clear(); }
    }
    public class MockAdScreen : MonoBehaviour
    {
        public static MockAdScreen Instance;
        Canvas _canvas;
        Text _countdown, _status;
        GameObject _root;
        AdCallbacks _callbacks;
        Coroutine _routine;
        public void Play(AdPlacement placement, AdCallbacks callbacks, MockAdOutcome outcome = MockAdOutcome.Success, float delay = 3f)
        {
            if (_callbacks != null) Finish("replaced");
            Build();
            _callbacks = callbacks;
            _root.SetActive(true);
            callbacks.Opened?.Invoke();
            callbacks.Impression?.Invoke();
            _routine = StartCoroutine(Run(placement, outcome, delay));
        }
        void Build()
        {
            if (_canvas != null) return;
            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 500;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f;
            canvasGo.AddComponent<GraphicRaycaster>();
            _root = new GameObject("Root", typeof(RectTransform));
            _root.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _root.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.04f, 0.97f);
            var panel = Ui.MakePanel(_root.transform, "AdPanel", 920, 760, Palette.PanelFill, Palette.Violet);
            Ui.MakeLabel(panel, "TEST AD", 54, Palette.TextDim, TextAnchor.MiddleCenter, 0, 275, 850, 70);
            Ui.MakeLabel(panel, "REWARDED VIDEO", 70, Palette.Violet, TextAnchor.MiddleCenter, 0, 180, 850, 90);
            Ui.MakeLabel(panel, "Editor / Development only", 30, Palette.TextDim, TextAnchor.MiddleCenter, 0, 90, 850, 55);
            _status = Ui.MakeLabel(panel, "WATCHING...", 35, Palette.TextMain, TextAnchor.MiddleCenter, 0, 10, 850, 60);
            _countdown = Ui.MakeLabel(panel, "3", 110, Palette.Cyan, TextAnchor.MiddleCenter, 0, -105, 300, 130);
            Ui.MakeButton(panel, "SKIP (NO REWARD)", 28, Palette.PanelFillSoft, Palette.TextDim, () => Finish(null), -205, -245, 400, 100);
            Ui.MakeButton(panel, "CLOSE", 28, Palette.PanelFillSoft, Palette.TextDim, () => Finish(null), 215, -245, 360, 100);
        }
        IEnumerator Run(AdPlacement placement, MockAdOutcome outcome, float delay)
        {
            _status.text = placement.ToString().ToUpperInvariant() + " - WATCHING...";
            float remaining = Mathf.Max(0.05f, delay);
            while (remaining > 0f)
            {
                _countdown.text = Mathf.CeilToInt(remaining).ToString();
                yield return null;
                remaining -= Time.unscaledDeltaTime;
            }
            if (outcome == MockAdOutcome.Success)
            {
                _callbacks?.Reward?.Invoke();
                _countdown.text = "OK";
                _status.text = "REWARD GRANTED";
                yield return new WaitForSecondsRealtime(0.7f);
            }
            _routine = null;
            Finish(outcome == MockAdOutcome.Failed ? "mock_failure" : null);
        }
        void Finish(string error)
        {
            var callbacks = _callbacks;
            if (callbacks == null) return;
            _callbacks = null;
            if (_routine != null) { StopCoroutine(_routine); _routine = null; }
            if (_root != null) _root.SetActive(false);
            if (error != null) callbacks.Failed?.Invoke(error); else callbacks.Closed?.Invoke();
        }
        void Update() { if (_callbacks != null && Input.GetKeyDown(KeyCode.Escape)) Finish(null); }
        void OnDisable() { Finish("mock_disabled"); }
        void OnDestroy() { Finish("mock_destroyed"); if (Instance == this) Instance = null; }
    }
#endif

#if EDGERUSH_ADMOB && (UNITY_ANDROID || UNITY_IOS)
    /// <summary>
    /// Google Mobile Ads adapter. Verify the installed SDK against the official rewarded guide;
    /// privacy/UMP integration is a release prerequisite. AdManager alone schedules loads.
    /// </summary>
    public class AdMobProvider : IAdProvider
    {
        readonly Dictionary<AdPlacement, GoogleMobileAds.Api.RewardedAd> _ads = new Dictionary<AdPlacement, GoogleMobileAds.Api.RewardedAd>();
        readonly Dictionary<AdPlacement, int> _generations = new Dictionary<AdPlacement, int>();
        GoogleMobileAds.Api.RewardedAd _presenting;
        bool _disposed, _initialized, _initializing;
        Action<bool, string> _initializeCallback;
        public string Name => "AdMob";
        public void Initialize(Action<bool, string> completed)
        {
            if (_initialized) { completed(true, null); return; }
            _initializeCallback = completed;
            if (_initializing) return;
            _initializing = true;
            GoogleMobileAds.Api.MobileAds.RaiseAdEventsOnUnityMainThread = true;
            GoogleMobileAds.Api.MobileAds.Initialize(status =>
            {
                if (_disposed) return;
                _initializing = false;
                _initialized = status != null;
                _initializeCallback?.Invoke(_initialized, _initialized ? null : "null_status");
                _initializeCallback = null;
            });
        }
        public bool IsReady(AdPlacement placement)
        {
            GoogleMobileAds.Api.RewardedAd ad;
            return !_disposed && _ads.TryGetValue(placement, out ad) && ad != null && ad.CanShowAd();
        }
        public void Clear(AdPlacement placement)
        {
            int generation; _generations.TryGetValue(placement, out generation);
            _generations[placement] = generation + 1;
            GoogleMobileAds.Api.RewardedAd old;
            if (_ads.TryGetValue(placement, out old) && old != null) old.Destroy();
            _ads.Remove(placement);
        }
        public void Load(AdPlacement placement, Action<bool, string> completed)
        {
            Clear(placement);
            int generation = _generations[placement];
            string id;
            if (!AdConfiguration.UnitIds.TryGetValue(placement, out id) || string.IsNullOrEmpty(id))
            { completed(false, "missing_unit_id"); return; }
            GoogleMobileAds.Api.RewardedAd.Load(id, new GoogleMobileAds.Api.AdRequest(), (ad, error) =>
            {
                if (_disposed || generation != _generations[placement]) { ad?.Destroy(); return; }
                if (error != null || ad == null) { ad?.Destroy(); completed(false, error != null ? error.GetMessage() : "null_ad"); return; }
                _ads[placement] = ad;
                completed(true, null);
            });
        }
        public void Show(AdPlacement placement, AdCallbacks callbacks)
        {
            GoogleMobileAds.Api.RewardedAd ad;
            if (!_ads.TryGetValue(placement, out ad) || ad == null || !ad.CanShowAd())
            { callbacks.Failed?.Invoke("not_ready_at_show"); return; }
            _ads.Remove(placement);
            _presenting = ad;
            bool closed = false;
            Action cleanup = () =>
            {
                if (closed) return;
                closed = true;
                if (ReferenceEquals(_presenting, ad)) _presenting = null;
                ad.Destroy();
            };
            ad.OnAdFullScreenContentOpened += () => callbacks.Opened?.Invoke();
            ad.OnAdImpressionRecorded += () => callbacks.Impression?.Invoke();
            ad.OnAdPaid += value => callbacks.Paid?.Invoke(value.Value, value.CurrencyCode, value.Precision.ToString());
            ad.OnAdFullScreenContentClosed += () => { if (closed) return; cleanup(); callbacks.Closed?.Invoke(); };
            ad.OnAdFullScreenContentFailed += error => { if (closed) return; cleanup(); callbacks.Failed?.Invoke(error.GetMessage()); };
            try { ad.Show(reward => callbacks.Reward?.Invoke()); } catch { cleanup(); throw; }
        }
        public void Dispose()
        {
            _disposed = true;
            foreach (var p in AdManager.AllPlacements) Clear(p);
            _presenting?.Destroy();
            _presenting = null;
        }
    }
#endif
}
