using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeRush
{
    public enum AdPlacement
    {
        Revive,
        DoubleCoins,
        DailyBonus,
        SkinUnlock,
    }

    public interface IAdProvider
    {
        string Name { get; }
        void Initialize();
        bool IsReady(AdPlacement placement);
        void Load(AdPlacement placement);
        void Show(AdPlacement placement, Action<bool> completed);
    }

    /// <summary>
    /// Single entry point for rewarded video. Placements are preloaded, buttons must query
    /// <see cref="Available"/> (never show a button for an ad that cannot play), and every
    /// show is logged. The provider is swappable: mock in the editor, AdMob in release builds.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager I { get; private set; }

        IAdProvider _provider;
        bool _showing;
        float _lastShownAt = -999f;

        public string ProviderName => _provider != null ? _provider.Name : "none";
        public bool Initialized { get; private set; }

        public static readonly AdPlacement[] AllPlacements =
        {
            AdPlacement.Revive, AdPlacement.DoubleCoins, AdPlacement.DailyBonus, AdPlacement.SkinUnlock,
        };

        public void Init()
        {
            I = this;
            _provider = CreateProvider();
            _provider.Initialize();
            Initialized = true;
            LoadAll();
        }

        IAdProvider CreateProvider()
        {
#if EDGERUSH_ADMOB && (UNITY_ANDROID || UNITY_IOS)
            if (AdMobProvider.IsConfigured()) return new AdMobProvider();
#endif
            return new MockAdProvider();
        }

        public void LoadAll()
        {
            if (_provider == null) return;
            foreach (var p in AllPlacements) _provider.Load(p);
        }

        /// <summary>True when the placement can actually be shown right now.</summary>
        public bool Available(AdPlacement placement)
        {
            return Initialized && !_showing && _provider != null && _provider.IsReady(placement);
        }

        public bool AnyReady
        {
            get
            {
                foreach (var p in AllPlacements)
                    if (Available(p)) return true;
                return false;
            }
        }

        /// <summary>
        /// Shows the ad. The callback receives true only when the reward was earned. Safe to
        /// call when nothing is ready: it reports false immediately.
        /// </summary>
        public void Show(AdPlacement placement, Action<bool> onResult)
        {
            if (_showing)
            {
                if (onResult != null) onResult(false);
                return;
            }
            if (!Available(placement))
            {
                Analytics.AdNotReady(placement.ToString());
                if (onResult != null) onResult(false);
                return;
            }

            _showing = true;
            _lastShownAt = Time.unscaledTime;
            Analytics.AdImpression(placement.ToString());
            if (AudioDirector.I != null) AudioDirector.I.DuckForAd(true);

            _provider.Show(placement, earned =>
            {
                _showing = false;
                if (AudioDirector.I != null) AudioDirector.I.DuckForAd(false);
                if (earned)
                {
                    Analytics.AdComplete(placement.ToString());
                    if (GameApp.I != null) GameApp.I.Save.totalAdViews++;
                }
                else
                {
                    Analytics.AdFailed(placement.ToString(), "closed");
                }
                LoadAll();
                if (onResult != null) onResult(earned);
            });
        }

        public float SecondsSinceLastAd => Time.unscaledTime - _lastShownAt;
    }

    /// <summary>
    /// Test ad used in the editor and in any build without an ad SDK. It behaves like a real
    /// rewarded video: a fixed watch period, then the reward, with an explicit skip path.
    /// </summary>
    public class MockAdProvider : IAdProvider
    {
        public string Name => "MockRewarded";

        public void Initialize()
        {
            Debug.Log("[Ads] Mock provider active - replace with AdMob before release.");
        }

        public bool IsReady(AdPlacement placement) => true;
        public void Load(AdPlacement placement) { }

        public void Show(AdPlacement placement, Action<bool> completed)
        {
            if (MockAdScreen.Instance == null)
            {
                var go = new GameObject("MockAdScreen");
                UnityEngine.Object.DontDestroyOnLoad(go);
                MockAdScreen.Instance = go.AddComponent<MockAdScreen>();
            }
            MockAdScreen.Instance.Play(placement, completed);
        }
    }

    public class MockAdScreen : MonoBehaviour
    {
        public static MockAdScreen Instance;

        Canvas _canvas;
        Text _countdown;
        Text _status;
        GameObject _root;

        public void Play(AdPlacement placement, Action<bool> completed)
        {
            Build();
            _root.SetActive(true);
            _canvas.sortingOrder = 500;
            StartCoroutine(Run(placement, completed));
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
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _root = new GameObject("Root", typeof(RectTransform));
            _root.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)_root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var bg = _root.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.02f, 0.04f, 0.97f);
            bg.raycastTarget = true;

            var panel = Ui.MakePanel(_root.transform, "AdPanel", 820, 620, Palette.PanelFill, Palette.Violet);
            Ui.MakeLabel(panel, "TEST AD", 54, Palette.TextDim, TextAnchor.MiddleCenter, 0, 200, 760, 60);
            Ui.MakeLabel(panel, "REWARDED VIDEO", 84, Palette.Violet, TextAnchor.MiddleCenter, 0, 120, 760, 100);
            Ui.MakeLabel(panel, "Placeholder shown in the editor.\nThe release build plays an AdMob ad here.",
                30, Palette.TextDim, TextAnchor.MiddleCenter, 0, 20, 720, 90);

            _status = Ui.MakeLabel(panel, "WATCHING...", 40, Palette.TextMain, TextAnchor.MiddleCenter, 0, -90, 760, 50);
            _countdown = Ui.MakeLabel(panel, "3", 120, Palette.Cyan, TextAnchor.MiddleCenter, 0, -190, 300, 130);

            Ui.MakeButton(panel, "SKIP (NO REWARD)", 26, Palette.PanelFillSoft, Palette.TextDim, () =>
            {
                StopAllCoroutines();
                Finish(false);
            }, 0, -260, 420, 70, 1f);
        }

        IEnumerator Run(AdPlacement placement, Action<bool> completed)
        {
            _status.text = placement.ToString().ToUpperInvariant() + " - WATCHING...";
            float remaining = 3f;
            while (remaining > 0f)
            {
                _countdown.text = Mathf.CeilToInt(remaining).ToString();
                yield return null;
                remaining -= Time.unscaledDeltaTime;
            }
            _countdown.text = "OK";
            _status.text = "REWARD GRANTED";
            yield return new WaitForSecondsRealtime(0.7f);
            Finish(true, completed);
        }

        void Finish(bool reward, Action<bool> completed = null)
        {
            if (_root != null) _root.SetActive(false);
            if (completed != null) completed(reward);
        }
    }

#if EDGERUSH_ADMOB && (UNITY_ANDROID || UNITY_IOS)
    /// <summary>
    /// Google AdMob rewarded video adapter (Google Mobile Ads Unity plugin v9.x).
    /// Enable by installing the plugin, adding the scripting define EDGERUSH_ADMOB and
    /// filling the unit ids below. This file is excluded from compilation otherwise.
    /// </summary>
    public class AdMobProvider : IAdProvider
    {
        // Replace with your own rewarded unit ids from the AdMob console.
        static readonly Dictionary<AdPlacement, string> UnitIds = new Dictionary<AdPlacement, string>
        {
            { AdPlacement.Revive,      "ca-app-pub-3940256099942544/5224354917" },
            { AdPlacement.DoubleCoins, "ca-app-pub-3940256099942544/5224354917" },
            { AdPlacement.DailyBonus,  "ca-app-pub-3940256099942544/5224354917" },
            { AdPlacement.SkinUnlock,  "ca-app-pub-3940256099942544/5224354917" },
        };

        public static bool IsConfigured()
        {
            foreach (var kv in UnitIds)
                if (!string.IsNullOrEmpty(kv.Value)) return true;
            return false;
        }

        readonly Dictionary<AdPlacement, GoogleMobileAds.Api.RewardedAd> _ads =
            new Dictionary<AdPlacement, GoogleMobileAds.Api.RewardedAd>();

        public string Name => "AdMob";

        public void Initialize()
        {
            GoogleMobileAds.Api.MobileAds.Initialize(status =>
                Debug.Log("[Ads] AdMob init: " + status));
            foreach (var p in AdManager.AllPlacements) Load(p);
        }

        public bool IsReady(AdPlacement placement)
        {
            GoogleMobileAds.Api.RewardedAd ad;
            return _ads.TryGetValue(placement, out ad) && ad != null && ad.CanShowAd();
        }

        public void Load(AdPlacement placement)
        {
            if (!UnitIds.ContainsKey(placement)) return;

            GoogleMobileAds.Api.RewardedAd old;
            if (_ads.TryGetValue(placement, out old) && old != null) old.Destroy();
            _ads[placement] = null;

            GoogleMobileAds.Api.RewardedAd.Load(UnitIds[placement],
                new GoogleMobileAds.Api.AdRequest(),
                (ad, error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogWarning("[Ads] load failed " + placement + ": " + (error != null ? error.GetMessage() : "null"));
                        return;
                    }
                    _ads[placement] = ad;
                });
        }

        public void Show(AdPlacement placement, Action<bool> completed)
        {
            GoogleMobileAds.Api.RewardedAd ad;
            if (!_ads.TryGetValue(placement, out ad) || ad == null || !ad.CanShowAd())
            {
                if (completed != null) completed(false);
                return;
            }

            bool earned = false;
            ad.OnAdFullScreenContentClosed += () =>
            {
                Load(placement);
                if (completed != null) completed(earned);
            };
            ad.OnAdFullScreenContentFailed += err =>
            {
                Load(placement);
                if (completed != null) completed(false);
            };
            ad.Show(reward => { earned = true; });
            _ads[placement] = null;
        }
    }
#endif
}
