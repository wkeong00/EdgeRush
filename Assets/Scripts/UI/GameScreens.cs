using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeRush
{
    // ============================================================== in-game HUD
    public class HudScreen : UiScreen
    {
        Text _score;
        Text _coins;
        Text _combo;
        Image _comboBar;
        RectTransform _comboRoot;
        Text _close;
        RectTransform _closeRoot;
        Button _pause;
        int _lastScore = -1, _lastCoins = -1, _lastCombo = -1;

        protected override void Construct()
        {
            Group.blocksRaycasts = false;

            _score = Ui.MakeLabel(Root, "0", 76, Palette.TextMain, TextAnchor.MiddleLeft, -160, 850, 420, 100);
            Ui.AddShadow(_score, 0.7f);
            _score.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Ui.Anchor(_score.rectTransform, new Vector2(0, 1), 238, -96, 380, 100);

            var coinPill = Ui.MakePanel(Root, "Coins", 280, 78, new Color(0.05f, 0.07f, 0.13f, 0.75f), Palette.Gold, 0.55f);
            Ui.Anchor(coinPill, Vector2.one, -306, -96, 260, 78);
            coinPill.GetComponent<Image>().raycastTarget = false;
            foreach (var g in coinPill.GetComponentsInChildren<Image>()) g.raycastTarget = false;
            var icon = Ui.AddImage(coinPill, "CoinIcon", Art.Coin(), Color.white);
            Ui.Place(icon.rectTransform, -100, 0, 52, 52);
            _coins = Ui.MakeLabel(coinPill, "0", 44, Palette.Gold, TextAnchor.MiddleLeft, -60, 0, 170, 60);

            Ui.MakeLabel(coinPill, "RUN", 18, Palette.TextDim, TextAnchor.MiddleLeft, 25, -23, 150, 20);
            _coins.fontSize = 36;
            _coins.rectTransform.anchoredPosition = new Vector2(25, 12);
            _coins.rectTransform.sizeDelta = new Vector2(150, 44);
            _pause = Ui.MakeIconButton(Root, "Pause", Art.IconPause(), Palette.TextMain, () => GameApp.I.PauseGame(), 0, 0, 104);
            Ui.Anchor((RectTransform)_pause.transform, Vector2.one, -96, -96, 104, 104);

            _comboRoot = Ui.Rect("Combo", Root);
            Ui.Top(_comboRoot, 150, 300, 110);
            _combo = Ui.MakeLabel(_comboRoot, "x1", 86, Palette.Cyan, TextAnchor.MiddleCenter, 0, 18, 300, 100);
            Ui.AddOutline(_combo, new Color(0.02f, 0.08f, 0.14f, 0.9f), 4f);
            _comboBar = Ui.MakeBar(_comboRoot, 0, -40, 240, 14, new Color(0.1f, 0.14f, 0.24f, 0.85f), Palette.Cyan);
            _comboRoot.gameObject.SetActive(false);

            _closeRoot = Ui.Rect("CloseCall", Root);
            Ui.Place(_closeRoot, 0, 120, 800, 200);
            _close = Ui.MakeLabel(_closeRoot, "CLOSE!", 78, Palette.Cyan, TextAnchor.MiddleCenter, 0, 0, 800, 130);
            Ui.AddOutline(_close, new Color(0.02f, 0.06f, 0.12f, 0.95f), 5f);
            _closeRoot.gameObject.SetActive(false);
        }

        public void SetScore(int score)
        {
            if (score == _lastScore) return;
            _lastScore = score;
            _score.text = score.ToString("N0");
        }

        public void SetCoins(int coins)
        {
            if (coins == _lastCoins) return;
            _lastCoins = coins;
            _coins.text = coins.ToString();
            Hub.Pop(_coins.transform, 1.35f, 0.16f);
        }

        public void SetCombo(int level, float progress)
        {
            bool show = level > 1;
            if (_comboRoot.gameObject.activeSelf != show) _comboRoot.gameObject.SetActive(show);
            if (show)
            {
                if (level != _lastCombo)
                {
                    _lastCombo = level;
                    _combo.text = "x" + level;
                    _combo.color = level >= 5 ? Palette.Gold : level >= 4 ? Palette.Magenta : level >= 3 ? Palette.Violet : Palette.Cyan;
                    Hub.Pop(_combo.transform, 1.8f, 0.22f);
                }
                Ui.SetBar(_comboBar, progress);
            }
            else
            {
                _lastCombo = 1;
            }
        }

        public void PopClose(int points, int comboLevel)
        {
            string text = comboLevel >= 5 ? "INSANE!" : comboLevel >= 4 ? "UNREAL!" : comboLevel >= 3 ? "CRAZY!" : "CLOSE!";
            Color color = comboLevel >= 5 ? Palette.Gold : comboLevel >= 4 ? Palette.Magenta : comboLevel >= 3 ? Palette.Violet : Palette.Cyan;
            _close.text = text;
            _close.color = color;
            _closeRoot.gameObject.SetActive(true);
            _closeRoot.SetAsLastSibling();
            Hub.Pop(_closeRoot, 1.2f, 0.2f);
            Hub.StartCoroutine(HideCloseAfter(0.45f));
            Hub.SpawnFloating("+" + points, new Vector2(0f, 20f), color, 52);
        }

        IEnumerator HideCloseAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (_closeRoot != null) _closeRoot.gameObject.SetActive(false);
        }

        public void ShowRunUi(bool show)
        {
            _score.gameObject.SetActive(show);
            _coins.gameObject.SetActive(show);
            _pause.gameObject.SetActive(show);
            if (!show)
            {
                _comboRoot.gameObject.SetActive(false);
                _closeRoot.gameObject.SetActive(false);
            }
            else
            {
                _lastScore = -1;
                _lastCoins = -1;
                _lastCombo = -1;
            }
        }
    }

    // ================================================================== pause
    public class PauseScreen : UiScreen
    {
        Text _score;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.72f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.FitPanel(Root, "Panel", 760, 860, Palette.Cyan);

            Ui.MakeLabel(panel, "PAUSED", 72, Palette.TextMain, TextAnchor.MiddleCenter, 0, 300, 640, 100);
            _score = Ui.MakeLabel(panel, "SCORE 0", 40, Palette.TextDim, TextAnchor.MiddleCenter, 0, 200, 640, 60);

            Ui.MakeButton(panel, "RESUME", 46, new Color(0.05f, 0.22f, 0.28f, 0.95f), Palette.Cyan,
                () => GameApp.I.ResumeGame(), 0, 60, 560, 120, 1f);
            Ui.MakeButton(panel, "RESTART", 42, Palette.PanelFillSoft, Palette.TextMain,
                () => GameApp.I.RestartRun(), 0, -90, 560, 110, 0.6f);
            Ui.MakeButton(panel, "QUIT TO MENU", 36, Palette.PanelFillSoft, Palette.TextDim,
                () => GameApp.I.QuitToMenu(), 0, -220, 560, 100, 0.35f);

            Ui.MakeLabel(panel, "Music and effects can be changed in Settings.", 26, Palette.TextDim,
                TextAnchor.MiddleCenter, 0, -340, 640, 60);
        }

        public override void Refresh()
        {
            if (GameApp.I != null && GameApp.I.Run != null)
                _score.text = "SCORE " + GameApp.I.Run.Score.ToString("N0");
        }
    }

    // ================================================================= revive
    public class ReviveScreen : UiScreen
    {
        Text _countdown;
        Text _multiplier;
        Text _adLabel;
        Button _adButton;
        float _remaining;
        bool _active;
        Coroutine _tickRoutine;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.78f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.FitPanel(Root, "Panel", 800, 980, Palette.Magenta);

            Ui.MakeLabel(panel, "KEEP GOING?", 72, Palette.TextMain, TextAnchor.MiddleCenter, 0, 350, 700, 100);
            _multiplier = Ui.MakeLabel(panel, "", 34, Palette.Magenta, TextAnchor.MiddleCenter, 0, 260, 700, 60);

            _countdown = Ui.MakeLabel(panel, "5", 160, Palette.Gold, TextAnchor.MiddleCenter, 0, 100, 300, 180);
            Ui.AddOutline(_countdown, new Color(0.1f, 0.05f, 0f, 0.9f), 5f);

            _adButton = Ui.MakeButton(panel, "WATCH AD & REVIVE", 40, new Color(0.30f, 0.06f, 0.18f, 0.95f), Palette.Magenta,
                () => { if (_active) GameApp.I.WatchReviveAd(); }, 0, -100, 640, 130, 1f, Palette.Magenta, Art.IconVideo());
            _adLabel = _adButton.GetComponentInChildren<Text>();

            Ui.MakeButton(panel, "NO THANKS", 30, Palette.PanelFillSoft, Palette.TextDim,
                () => { if (_active) GameApp.I.DeclineRevive(); }, 0, -260, 420, 84, 0.3f);

            Ui.MakeLabel(panel, "One revive per run. Clears traffic ahead.\nBrief collision protection, then normal speed resumes.",
                24, Palette.TextDim, TextAnchor.MiddleCenter, 0, -370, 690, 90, false, true);
        }

        public void Present(float seconds, int comboLevel)
        {
            if (_tickRoutine != null) Hub.StopCoroutine(_tickRoutine);
            _remaining = seconds;
            _active = true;
            _countdown.text = Mathf.CeilToInt(_remaining).ToString();
            _multiplier.text = comboLevel > 1
                ? "Revive keeps your x" + comboLevel + " multiplier."
                : "Revive and keep the run alive.";

            bool canShow = GameApp.I.Ads.Available(AdPlacement.Revive);
            _adButton.gameObject.SetActive(true);
            _adButton.interactable = canShow;
            if (!canShow)
                _adLabel.text = GameApp.I.Ads.UnavailableReason(AdPlacement.Revive);
            else _adLabel.text = "WATCH AD & REVIVE";

            Show();
            _tickRoutine = Hub.StartCoroutine(Tick());
        }

        IEnumerator Tick()
        {
            while (_remaining > 0f && _active)
            {
                if (GameApp.I.Suspended) { yield return null; continue; }
                _remaining -= Time.unscaledDeltaTime;
                _countdown.text = Mathf.Max(0, Mathf.CeilToInt(_remaining)).ToString();
                yield return null;
            }
            if (_active)
            {
                _active = false;
                _tickRoutine = null;
                GameApp.I.DeclineRevive();
            }
        }

        public void Close()
        {
            Hide();
        }

        public override void Hide(bool instant = false)
        {
            _active = false;
            if (_tickRoutine != null) { Hub.StopCoroutine(_tickRoutine); _tickRoutine = null; }
            base.Hide(instant);
        }
    }

    // ================================================================ results
    public class ResultsScreen : UiScreen
    {
        Text _score, _best, _newBest, _metres, _nearMisses, _coins, _delta, _peakCombo, _adReason;
        Button _doubleButton;
        Text _doubleLabel;
        RunSummary _summary;
        bool _canDouble;
        bool _bonusClaimed;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.7f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.FitPanel(Root, "Panel", 880, 1400, Palette.Cyan);

            Ui.MakeLabel(panel, "RUN OVER", 44, Palette.TextDim, TextAnchor.MiddleCenter, 0, 560, 700, 60);

            _score = Ui.MakeLabel(panel, "0", 150, Palette.TextMain, TextAnchor.MiddleCenter, 0, 420, 800, 180);
            _score.resizeTextForBestFit = true;
            _score.resizeTextMinSize = 72;
            _score.resizeTextMaxSize = 150;
            Ui.AddShadow(_score, 0.75f);

            _newBest = Ui.MakeLabel(panel, "NEW BEST!", 44, Palette.Gold, TextAnchor.MiddleCenter, 0, 320, 700, 60);
            _best = Ui.MakeLabel(panel, "BEST 0", 30, Palette.TextDim, TextAnchor.MiddleCenter, 0, 265, 700, 50);
            _delta = Ui.MakeLabel(panel, "", 30, Palette.Magenta, TextAnchor.MiddleCenter, 0, 215, 760, 50);

            var divider = Ui.AddFill(panel, "Divider", new Color(1f, 1f, 1f, 0.10f));
            Ui.Place(divider.rectTransform, 0, 165, 700, 3);

            _metres = Ui.MakeLabel(panel, "", 30, Palette.TextDim, TextAnchor.MiddleCenter, -180, 102, 320, 60);
            _nearMisses = Ui.MakeLabel(panel, "", 34, Palette.Cyan, TextAnchor.MiddleCenter, 180, 102, 360, 60);
            _peakCombo = Ui.MakeLabel(panel, "", 32, Palette.Magenta, TextAnchor.MiddleCenter, 0, 44, 700, 50);

            var coinIcon = Ui.AddImage(panel, "Coin", Art.Coin(), Color.white);
            Ui.Place(coinIcon.rectTransform, -80, -28, 54, 54);
            _coins = Ui.MakeLabel(panel, "+0", 46, Palette.Gold, TextAnchor.MiddleLeft, 92, -28, 250, 70);

            _doubleButton = Ui.MakeButton(panel, "DOUBLE COINS", 32, new Color(0.26f, 0.16f, 0.02f, 0.95f), Palette.Gold,
                () => GameApp.I.WatchDoubleCoins(), 0, -310, 660, 110, 0.7f, Palette.Gold, Art.IconVideo());
            _doubleLabel = _doubleButton.GetComponentInChildren<Text>();
            _adReason = Ui.MakeLabel(panel, "", 24, Palette.TextDim, TextAnchor.MiddleCenter, 0, -396, 760, 48);

            Ui.MakeButton(panel, Strings.Get("results.again"), 48, new Color(0.04f, 0.26f, 0.30f, 0.96f), Palette.Cyan,
                () => GameApp.I.RestartRun(), 0, -150, 660, 140, 1f);
            Ui.MakeButton(panel, "HOME", 36, Palette.PanelFillSoft, Palette.TextDim,
                () => GameApp.I.QuitToMenu(), 0, -520, 660, 100, 0.35f);
        }

        public void Present(RunSummary summary, int bestScore, bool newBest, int coinsEarned, bool canDouble)
        {
            _summary = summary;
            _canDouble = canDouble;
            _bonusClaimed = false;

            _score.text = summary.score.ToString("N0");
            _best.text = "BEST " + Mathf.Max(bestScore, summary.score).ToString("N0");
            _newBest.gameObject.SetActive(newBest);
            _delta.text = newBest
                ? "You beat your old record."
                : (bestScore - summary.score > 0
                    ? Mathf.Min(bestScore - summary.score, 99999).ToString("N0") + " POINTS TO YOUR BEST"
                    : "");
            _metres.text = summary.metres.ToString("N0") + " m";
            _nearMisses.text = summary.nearMisses + " close calls";
            _peakCombo.text = "PEAK MULTIPLIER  x" + summary.comboLevel;
            _coins.text = "+" + coinsEarned;
            _doubleButton.gameObject.SetActive(canDouble);
            RefreshAdState();
        }

        public void RefreshAdState()
        {
            if (_doubleButton == null || GameApp.I == null || GameApp.I.Ads == null) return;
            if (_bonusClaimed) return;
            bool ready = _canDouble && GameApp.I.Ads.Available(AdPlacement.DoubleCoins);
            _doubleButton.interactable = ready;
            _doubleLabel.text = "WATCH AD  +" + _summary.coins + " COINS";
            _adReason.text = _canDouble && !ready ? GameApp.I.Ads.UnavailableReason(AdPlacement.DoubleCoins) : "";
        }

        public void MarkDoubled(int newTotal)
        {
            _coins.text = "+" + newTotal;
            _canDouble = false;
            _bonusClaimed = true;
            _doubleButton.interactable = false;
            _doubleLabel.text = "BONUS CLAIMED";
            _adReason.text = "";
            Hub.Pop(_coins.transform, 1.5f, 0.25f);
        }
    }
}




