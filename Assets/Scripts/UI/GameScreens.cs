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

            var coinPill = Ui.MakePanel(Root, "Coins", 280, 78, new Color(0.05f, 0.07f, 0.13f, 0.75f), Palette.Gold, 0.55f);
            Ui.Place(coinPill, 300, 850, 260, 78);
            coinPill.GetComponent<Image>().raycastTarget = false;
            foreach (var g in coinPill.GetComponentsInChildren<Image>()) g.raycastTarget = false;
            var icon = Ui.AddImage(coinPill, "CoinIcon", Art.Coin(), Color.white);
            Ui.Place(icon.rectTransform, -100, 0, 52, 52);
            _coins = Ui.MakeLabel(coinPill, "0", 44, Palette.Gold, TextAnchor.MiddleLeft, -60, 0, 170, 60);

            _pause = Ui.MakeIconButton(Root, "Pause", Art.IconPause(), Palette.TextMain, () => GameApp.I.PauseGame(), 420, 725, 96);

            _comboRoot = Ui.Rect("Combo", Root);
            Ui.Place(_comboRoot, 0, 770, 320, 120);
            _combo = Ui.MakeLabel(_comboRoot, "x1", 86, Palette.Cyan, TextAnchor.MiddleCenter, 0, 18, 300, 100);
            Ui.AddOutline(_combo, new Color(0.02f, 0.08f, 0.14f, 0.9f), 4f);
            _comboBar = Ui.MakeBar(_comboRoot, 0, -40, 240, 14, new Color(0.1f, 0.14f, 0.24f, 0.85f), Palette.Cyan);
            _comboRoot.gameObject.SetActive(false);

            _closeRoot = Ui.Rect("CloseCall", Root);
            Ui.Place(_closeRoot, 0, 120, 800, 200);
            _close = Ui.MakeLabel(_closeRoot, "CLOSE!", 120, Palette.Cyan, TextAnchor.MiddleCenter, 0, 0, 800, 180);
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
            Hub.Pop(_closeRoot, 1.7f, 0.2f);
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

            var panel = Ui.MakePanel(Root, "Panel", 760, 860, Palette.PanelFill, Palette.Cyan, 0.9f);
            Ui.Place(panel, 0, 0, 760, 860);

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

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.78f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.MakePanel(Root, "Panel", 800, 940, Palette.PanelFill, Palette.Magenta, 0.95f);
            Ui.Place(panel, 0, 0, 800, 940);

            Ui.MakeLabel(panel, "KEEP GOING?", 72, Palette.TextMain, TextAnchor.MiddleCenter, 0, 350, 700, 100);
            _multiplier = Ui.MakeLabel(panel, "", 34, Palette.Magenta, TextAnchor.MiddleCenter, 0, 260, 700, 60);

            _countdown = Ui.MakeLabel(panel, "5", 160, Palette.Gold, TextAnchor.MiddleCenter, 0, 100, 300, 180);
            Ui.AddOutline(_countdown, new Color(0.1f, 0.05f, 0f, 0.9f), 5f);

            _adButton = Ui.MakeButton(panel, "WATCH AD & REVIVE", 40, new Color(0.30f, 0.06f, 0.18f, 0.95f), Palette.Magenta,
                () => { if (_active) GameApp.I.WatchReviveAd(); }, 0, -100, 640, 130, 1f, Palette.Magenta, Art.IconVideo());
            _adLabel = _adButton.GetComponentInChildren<Text>();

            Ui.MakeButton(panel, "NO THANKS", 30, Palette.PanelFillSoft, Palette.TextDim,
                () => { if (_active) GameApp.I.DeclineRevive(); }, 0, -260, 420, 84, 0.3f);

            Ui.MakeLabel(panel, "One revive per run.", 24, Palette.TextDim, TextAnchor.MiddleCenter, 0, -370, 600, 50);
        }

        public void Present(float seconds, int comboLevel)
        {
            _remaining = seconds;
            _active = true;
            _countdown.text = Mathf.CeilToInt(_remaining).ToString();
            _multiplier.text = comboLevel > 1
                ? "Revive keeps your x" + comboLevel + " multiplier."
                : "Revive and keep the run alive.";

            bool canShow = GameApp.I.Ads.Available(AdPlacement.Revive);
            _adButton.gameObject.SetActive(canShow);
            if (!canShow)
                _multiplier.text = "Ad unavailable right now - the run ends here.";

            Show();
            Hub.StartCoroutine(Tick());
        }

        IEnumerator Tick()
        {
            while (_remaining > 0f && _active)
            {
                _remaining -= Time.unscaledDeltaTime;
                _countdown.text = Mathf.Max(0, Mathf.CeilToInt(_remaining)).ToString();
                yield return null;
            }
            if (_active)
            {
                _active = false;
                GameApp.I.DeclineRevive();
            }
        }

        public void Close()
        {
            _active = false;
            Hide();
        }
    }

    // ================================================================ results
    public class ResultsScreen : UiScreen
    {
        Text _score, _best, _newBest, _metres, _nearMisses, _coins, _delta;
        Button _doubleButton;
        Text _doubleLabel;
        RunSummary _summary;
        bool _canDouble;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.7f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.MakePanel(Root, "Panel", 880, 1300, Palette.PanelFill, Palette.Cyan, 0.9f);
            Ui.Place(panel, 0, 0, 880, 1300);

            Ui.MakeLabel(panel, "RUN OVER", 44, Palette.TextDim, TextAnchor.MiddleCenter, 0, 560, 700, 60);

            _score = Ui.MakeLabel(panel, "0", 170, Palette.TextMain, TextAnchor.MiddleCenter, 0, 420, 800, 200);
            Ui.AddShadow(_score, 0.75f);

            _newBest = Ui.MakeLabel(panel, "NEW BEST!", 44, Palette.Gold, TextAnchor.MiddleCenter, 0, 320, 700, 60);
            _best = Ui.MakeLabel(panel, "BEST 0", 30, Palette.TextDim, TextAnchor.MiddleCenter, 0, 265, 700, 50);
            _delta = Ui.MakeLabel(panel, "", 30, Palette.Magenta, TextAnchor.MiddleCenter, 0, 215, 760, 50);

            var divider = Ui.AddFill(panel, "Divider", new Color(1f, 1f, 1f, 0.10f));
            Ui.Place(divider.rectTransform, 0, 165, 700, 3);

            _metres = Ui.MakeLabel(panel, "", 32, Palette.TextMain, TextAnchor.MiddleCenter, -180, 100, 320, 60);
            _nearMisses = Ui.MakeLabel(panel, "", 32, Palette.TextMain, TextAnchor.MiddleCenter, 180, 100, 320, 60);

            var coinIcon = Ui.AddImage(panel, "Coin", Art.Coin(), Color.white);
            Ui.Place(coinIcon.rectTransform, -80, 20, 54, 54);
            _coins = Ui.MakeLabel(panel, "+0", 52, Palette.Gold, TextAnchor.MiddleLeft, 10, 20, 300, 70);

            _doubleButton = Ui.MakeButton(panel, "DOUBLE COINS", 38, new Color(0.26f, 0.16f, 0.02f, 0.95f), Palette.Gold,
                () => GameApp.I.WatchDoubleCoins(), 0, -110, 660, 120, 1f, Palette.Gold, Art.IconVideo());
            _doubleLabel = _doubleButton.GetComponentInChildren<Text>();

            Ui.MakeButton(panel, "RUN AGAIN", 48, new Color(0.04f, 0.26f, 0.30f, 0.96f), Palette.Cyan,
                () => GameApp.I.RestartRun(), 0, -280, 660, 140, 1f);
            Ui.MakeButton(panel, "HOME", 36, Palette.PanelFillSoft, Palette.TextDim,
                () => GameApp.I.QuitToMenu(), 0, -430, 660, 100, 0.35f);
        }

        public void Present(RunSummary summary, int bestScore, bool newBest, int coinsEarned, bool canDouble)
        {
            _summary = summary;
            _canDouble = canDouble;

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
            _coins.text = "+" + coinsEarned;
            _doubleButton.gameObject.SetActive(canDouble);
            if (canDouble) _doubleLabel.text = "DOUBLE COINS";
        }

        public void MarkDoubled(int newTotal)
        {
            _coins.text = "+" + newTotal;
            _canDouble = false;
            _doubleButton.gameObject.SetActive(false);
            Hub.Pop(_coins.transform, 1.5f, 0.25f);
        }
    }
}




