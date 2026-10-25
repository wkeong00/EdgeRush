using UnityEngine;
using UnityEngine.UI;

namespace EdgeRush
{
    // ================================================================== garage
    public class GarageScreen : UiScreen
    {
        const string PreviewName = "Preview";

        int _index;
        Image _preview;
        Image _glow;
        Text _name, _blurb, _price, _coins, _progressLabel;
        Button _action, _adProgressButton;
        Image _progressBar;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.74f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.MakePanel(Root, "Panel", 900, 1320, Palette.PanelFill, Palette.Cyan, 0.9f);
            Ui.Place(panel, 0, 0, 900, 1320);

            Ui.MakeLabel(panel, "GARAGE", 60, Palette.TextMain, TextAnchor.MiddleCenter, 0, 540, 700, 90);

            var coinPill = Ui.MakePanel(panel, "Coins", 280, 76, new Color(0.05f, 0.07f, 0.13f, 0.7f), Palette.Gold, 0.5f);
            Ui.Place(coinPill, 0, 450, 280, 76);
            var icon = Ui.AddImage(coinPill, "CoinIcon", Art.Coin(), Color.white);
            Ui.Place(icon.rectTransform, -100, 0, 48, 48);
            _coins = Ui.MakeLabel(coinPill, "0", 40, Palette.Gold, TextAnchor.MiddleLeft, -55, 0, 170, 60);

            var glow = Ui.AddImage(panel, "Glow", Art.Glow(), new Color(0.3f, 0.9f, 1f, 0.25f));
            Ui.Place(glow.rectTransform, 0, 150, 560, 560);
            glow.raycastTarget = false;
            _glow = glow;

            _preview = Ui.AddImage(panel, PreviewName, Art.Car(Skins.All[0]), Color.white);
            Ui.Place(_preview.rectTransform, 0, 160, 280, 450);
            _preview.preserveAspect = true;
            _preview.raycastTarget = false;

            Ui.MakeIconButton(panel, "Prev", Art.IconArrowRight(), Palette.TextMain, () => Cycle(-1), -360, 160, 96, new Color(0.06f, 0.08f, 0.16f, 0.85f));
            Ui.MakeIconButton(panel, "Next", Art.IconArrowRight(), Palette.TextMain, () => Cycle(1), 360, 160, 96, new Color(0.06f, 0.08f, 0.16f, 0.85f));
            // the "prev" button reuses the arrow sprite, flipped
            var prevGlyph = panel.Find("Prev/Glyph");
            if (prevGlyph != null) prevGlyph.localScale = new Vector3(-1f, 1f, 1f);

            _name = Ui.MakeLabel(panel, "", 44, Palette.TextMain, TextAnchor.MiddleCenter, 0, -140, 760, 60);
            _blurb = Ui.MakeLabel(panel, "", 26, Palette.TextDim, TextAnchor.MiddleCenter, 0, -200, 760, 70, false, true);

            _price = Ui.MakeLabel(panel, "", 36, Palette.Gold, TextAnchor.MiddleCenter, 0, -290, 700, 60);

            _progressBar = Ui.MakeBar(panel, 0, -350, 520, 16, new Color(0.1f, 0.12f, 0.2f, 0.9f), Palette.Magenta);
            _progressLabel = Ui.MakeLabel(panel, "", 26, Palette.Magenta, TextAnchor.MiddleCenter, 0, -400, 700, 50);

            _action = Ui.MakeButton(panel, "SELECT", 42, new Color(0.04f, 0.26f, 0.30f, 0.96f), Palette.Cyan,
                OnAction, 0, -490, 640, 130, 1f);

            _adProgressButton = Ui.MakeButton(panel, "WATCH AD", 30, new Color(0.22f, 0.06f, 0.18f, 0.95f), Palette.Magenta,
                OnAdProgress, 0, -490, 640, 130, 1f, Palette.Magenta, Art.IconVideo());

            Ui.MakeButton(panel, "BACK", 34, Palette.PanelFillSoft, Palette.TextDim,
                () => Hide(), 0, -600, 260, 84, 0.35f);
        }

        public override void Refresh()
        {
            var save = GameApp.I.Save;
            if (_index < 0) _index = 0;
            if (_index >= Skins.All.Length) _index = Skins.All.Length - 1;
            var skin = Skins.All[_index];

            _coins.text = save.coins.ToString();
            _name.text = skin.name;
            _blurb.text = skin.blurb;
            _preview.sprite = Art.Car(skin);
            _glow.color = new Color(skin.glow.r, skin.glow.g, skin.glow.b, 0.26f);

            bool owned = save.Owns(skin.id);
            bool selected = save.selectedSkin == skin.id;

            if (skin.adUnlock && !owned)
            {
                _action.gameObject.SetActive(false);
                _adProgressButton.gameObject.SetActive(true);
                _progressBar.transform.parent.gameObject.SetActive(true);
                _progressLabel.gameObject.SetActive(true);
                _progressLabel.text = "WATCH " + Tuning.SkinAdCount + " VIDEOS TO UNLOCK  (" +
                                      Mathf.Min(save.adSkinProgress, Tuning.SkinAdCount) + "/" + Tuning.SkinAdCount + ")";
                Ui.SetBar(_progressBar, save.adSkinProgress / (float)Tuning.SkinAdCount);
                _price.text = "FREE - AD UNLOCK";
                _price.color = Palette.Magenta;
                _adProgressButton.interactable = GameApp.I.Ads.Available(AdPlacement.SkinUnlock);
            }
            else
            {
                _action.gameObject.SetActive(true);
                _adProgressButton.gameObject.SetActive(false);
                _progressBar.transform.parent.gameObject.SetActive(false);
                _progressLabel.gameObject.SetActive(false);

                var label = _action.GetComponentInChildren<Text>();
                if (owned)
                {
                    label.text = selected ? "SELECTED" : "SELECT";
                    _action.interactable = !selected;
                    _price.text = selected ? "IN USE" : "OWNED";
                    _price.color = selected ? Palette.Cyan : Palette.TextDim;
                }
                else
                {
                    label.text = "BUY  " + skin.price.ToString("N0");
                    _action.interactable = save.coins >= skin.price;
                    _price.text = skin.price.ToString("N0") + " COINS";
                    _price.color = save.coins >= skin.price ? Palette.Gold : Palette.Danger;
                }
            }
        }

        void Cycle(int dir)
        {
            _index = (_index + dir + Skins.All.Length) % Skins.All.Length;
            if (AudioDirector.I != null) AudioDirector.I.Play(Sfx.Click, 0.4f);
            Refresh();
            Hub.Pop(_preview.transform, 0.9f, 0.16f);
        }

        void OnAction()
        {
            var save = GameApp.I.Save;
            var skin = Skins.All[_index];
            if (save.Owns(skin.id))
            {
                GameApp.I.SelectSkin(skin);
                Hub.Toast.Show(skin.name + " SELECTED", Palette.Cyan);
            }
            else if (GameApp.I.BuySkin(skin))
            {
                Hub.Toast.Show(skin.name + " UNLOCKED", Palette.Gold);
                if (AudioDirector.I != null) AudioDirector.I.Play(Sfx.Buy);
            }
            else
            {
                Hub.Toast.Show("NOT ENOUGH COINS", Palette.Danger);
            }
            Refresh();
        }

        void OnAdProgress()
        {
            var skin = Skins.All[_index];
            if (!GameApp.I.Ads.Available(AdPlacement.SkinUnlock))
            {
                Hub.Toast.Show("AD NOT READY - TRY AGAIN", Palette.TextDim);
                return;
            }
            GameApp.I.Ads.Show(AdPlacement.SkinUnlock, earned =>
            {
                if (!earned) return;
                bool unlocked = Progression.AddAdSkinProgress(GameApp.I.Save);
                GameApp.I.SaveNow();
                Refresh();
                Hub.Toast.Show(unlocked ? "CHROME GHOST UNLOCKED!" : "PROGRESS SAVED", palette(unlocked));
            });
        }

        Color palette(bool unlocked) => unlocked ? Palette.Gold : Palette.Magenta;

        public void OpenOn(string skinId)
        {
            _index = Skins.IndexOf(skinId);
        }
    }

    // =================================================================== daily
    public class DailyScreen : UiScreen
    {
        readonly RectTransform[] _cells = new RectTransform[7];
        readonly Image[] _cellFills = new Image[7];
        readonly Text[] _cellLabels = new Text[7];
        Button _claim, _adButton;
        Text _streak, _adLabel;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.74f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.MakePanel(Root, "Panel", 920, 1000, Palette.PanelFill, Palette.Gold, 0.9f);
            Ui.Place(panel, 0, 0, 920, 1000);

            Ui.MakeLabel(panel, "DAILY REWARD", 58, Palette.TextMain, TextAnchor.MiddleCenter, 0, 400, 800, 90);
            _streak = Ui.MakeLabel(panel, "", 30, Palette.Gold, TextAnchor.MiddleCenter, 0, 320, 800, 60);

            for (int i = 0; i < 7; i++)
            {
                float x = -396f + i * 132f;
                var cell = Ui.MakePanel(panel, "Day" + (i + 1), 118, 170, new Color(0.07f, 0.08f, 0.16f, 0.9f), Palette.Gold, 0.25f);
                Ui.Place(cell, x, 150, 118, 170);
                _cells[i] = cell;
                _cellFills[i] = cell.GetComponent<Image>();

                Ui.MakeLabel(cell, "D" + (i + 1), 24, Palette.TextDim, TextAnchor.UpperCenter, 0, 54, 110, 40);
                var gift = Ui.AddImage(cell, "Gift", Art.IconGift(), Palette.Gold);
                Ui.Place(gift.rectTransform, 0, 8, 52, 52);
                _cellLabels[i] = Ui.MakeLabel(cell, Progression.DailyRewards[i].ToString(), 26, Palette.TextMain,
                    TextAnchor.LowerCenter, 0, -46, 110, 40);
            }

            _claim = Ui.MakeButton(panel, "CLAIM", 44, new Color(0.28f, 0.22f, 0.02f, 0.96f), Palette.Gold,
                () => GameApp.I.ClaimDaily(), 0, -120, 620, 130, 1f);

            _adButton = Ui.MakeButton(panel, "WATCH AD  +30", 34, new Color(0.22f, 0.06f, 0.18f, 0.95f), Palette.Magenta,
                () => GameApp.I.WatchDailyAd(), 0, -280, 620, 110, 0.9f, Palette.Magenta, Art.IconVideo());
            _adLabel = _adButton.GetComponentInChildren<Text>();

            Ui.MakeLabel(panel, "Come back every day. Missing a day resets the streak to day 1.",
                24, Palette.TextDim, TextAnchor.MiddleCenter, 0, -390, 800, 60);

            Ui.MakeButton(panel, "BACK", 34, Palette.PanelFillSoft, Palette.TextDim,
                () => Hide(), 0, -460, 260, 84, 0.35f);
        }

        public override void Refresh()
        {
            var save = GameApp.I.Save;
            int nextDay = Progression.NextDailyDay(save);
            bool canClaim = Progression.CanClaimDaily(save);

            _streak.text = canClaim
                ? "DAY " + nextDay + " OF 7 READY"
                : "COLLECTED TODAY - COME BACK TOMORROW";

            for (int i = 0; i < 7; i++)
            {
                int day = i + 1;
                bool isNext = canClaim && day == nextDay;
                bool collected = !canClaim && day <= save.dailyStreak;
                bool past = day < nextDay || (!canClaim && day == save.dailyStreak);

                _cellFills[i].color = isNext
                    ? new Color(0.30f, 0.24f, 0.04f, 0.95f)
                    : collected || past
                        ? new Color(0.06f, 0.14f, 0.16f, 0.9f)
                        : new Color(0.07f, 0.08f, 0.16f, 0.9f);
                _cellLabels[i].color = isNext ? Palette.Gold : collected || past ? Palette.Cyan : Palette.TextDim;
            }

            var claimText = _claim.GetComponentInChildren<Text>();
            claimText.text = canClaim ? "CLAIM  " + Progression.DailyRewardForDay(nextDay) : "CLAIMED";
            _claim.interactable = canClaim;

            int left = Progression.DailyAdsLeft(save);
            _adButton.interactable = left > 0 && GameApp.I.Ads.Available(AdPlacement.DailyBonus);
            _adLabel.text = left > 0 ? "WATCH AD  +" + Tuning.DailyAdCoins + "   (" + left + " LEFT)" : "NO ADS LEFT TODAY";
        }
    }

    // ================================================================= quests
    public class QuestsScreen : UiScreen
    {
        class Row
        {
            public RectTransform root;
            public Text title;
            public Text progress;
            public Image bar;
            public Button claim;
            public Text claimLabel;
            public Text reward;
        }

        readonly Row[] _rows = new Row[3];
        Text _header;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.74f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.MakePanel(Root, "Panel", 900, 1180, Palette.PanelFill, Palette.Violet, 0.9f);
            Ui.Place(panel, 0, 0, 900, 1180);

            Ui.MakeLabel(panel, "MISSIONS", 58, Palette.TextMain, TextAnchor.MiddleCenter, 0, 480, 800, 90);
            _header = Ui.MakeLabel(panel, "New missions every day at midnight.", 26, Palette.TextDim,
                TextAnchor.MiddleCenter, 0, 400, 800, 60);

            for (int i = 0; i < 3; i++)
            {
                var row = new Row();
                row.root = Ui.MakePanel(panel, "Quest" + i, 800, 210, new Color(0.06f, 0.07f, 0.14f, 0.92f), Palette.Violet, 0.25f);
                Ui.Place(row.root, 0, 220 - i * 250, 800, 210);

                row.title = Ui.MakeLabel(row.root, "", 34, Palette.TextMain, TextAnchor.MiddleLeft, 0, 56, 700, 50);
                row.bar = Ui.MakeBar(row.root, 0, 4, 700, 14, new Color(0.1f, 0.12f, 0.2f, 0.9f), Palette.Violet);
                row.progress = Ui.MakeLabel(row.root, "", 26, Palette.TextDim, TextAnchor.MiddleLeft, 0, -32, 420, 40);
                row.reward = Ui.MakeLabel(row.root, "", 26, Palette.Gold, TextAnchor.MiddleRight, 0, -32, 420, 40);

                row.claim = Ui.MakeButton(row.root, "CLAIM", 28, new Color(0.26f, 0.18f, 0.02f, 0.96f), Palette.Gold,
                    MakeClaimHandler(i), 290, -60, 180, 70, 0.8f);
                row.claimLabel = row.claim.GetComponentInChildren<Text>();
                _rows[i] = row;
            }

            Ui.MakeButton(panel, "BACK", 34, Palette.PanelFillSoft, Palette.TextDim,
                () => Hide(), 0, -510, 260, 84, 0.35f);
        }

        UnityEngine.Events.UnityAction MakeClaimHandler(int index)
        {
            return () =>
            {
                var save = GameApp.I.Save;
                if (index < 0 || index >= save.quests.Count) return;
                var q = save.quests[index];
                QuestDef def;
                if (!Progression.CanClaimQuest(q, out def)) return;
                int coins = Progression.ClaimQuest(save, q);
                GameApp.I.SaveNow();
                Hub.Toast.Show("+" + coins + " COINS", Palette.Gold);
                Refresh();
            };
        }

        public override void Refresh()
        {
            var save = GameApp.I.Save;
            Progression.EnsureQuests(save);

            int done = 0;
            for (int i = 0; i < _rows.Length; i++)
            {
                var row = _rows[i];
                if (i >= save.quests.Count)
                {
                    row.root.gameObject.SetActive(false);
                    continue;
                }
                row.root.gameObject.SetActive(true);
                var q = save.quests[i];
                var def = Progression.QuestById(q.id);
                row.title.text = def.title;
                int shown = Mathf.Min(q.progress, def.target);
                row.progress.text = shown + " / " + def.target;
                Ui.SetBar(row.bar, q.progress / (float)def.target);
                row.reward.text = "+" + def.reward + " COINS";

                bool ready = !q.claimed && q.progress >= def.target;
                if (ready) done++;
                row.claim.gameObject.SetActive(!q.claimed);
                row.claim.interactable = ready;
                row.claimLabel.text = ready ? "CLAIM" : "IN PROGRESS";
                row.claimLabel.color = ready ? Palette.Gold : Palette.TextDim;
                row.title.color = q.claimed ? Palette.TextDim : Palette.TextMain;
            }

            _header.text = done > 0 ? done + " MISSION(S) READY TO CLAIM" : "New missions every day at midnight.";
            _header.color = done > 0 ? Palette.Gold : Palette.TextDim;
        }
    }
}



