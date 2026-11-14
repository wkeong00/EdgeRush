using UnityEngine;
using UnityEngine.UI;

namespace EdgeRush
{
    public class GarageScreen : UiScreen
    {
        int _index;
        Image _preview, _glow, _progressBar;
        Text _name, _blurb, _price, _coins, _progressLabel, _adReason;
        Button _action, _adProgressButton;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0, 0, 0, 0.74f), true);
            Ui.Stretch(dim.rectTransform);
            var panel = Ui.FitPanel(Root, "Panel", 900, 1480, Palette.Cyan);
            var title = Ui.MakeLabel(panel, "GARAGE", 58, Palette.TextMain, TextAnchor.MiddleCenter, 0, 0, 740, 84);
            Ui.Top(title.rectTransform, 32, 740, 84);
            RectTransform wallet;
            _coins = Ui.Wallet(panel, out wallet);
            Ui.Top(wallet, 132, 300, 84);

            var previewArea = Ui.Rect("PreviewArea", panel);
            Ui.Stretch(previewArea, 40, 610, 40, 240);
            _glow = Ui.AddImage(previewArea, "Glow", Art.Glow(), new Color(0.3f, 0.9f, 1, 0.25f));
            Ui.Place(_glow.rectTransform, 0, 0, 500, 500);
            _preview = Ui.AddImage(previewArea, "Preview", Art.Car(Skins.All[0]), Color.white);
            Ui.Place(_preview.rectTransform, 0, 0, 250, 400);
            _preview.preserveAspect = true;
            var prev = Ui.MakeIconButton(previewArea, "Prev", Art.IconArrowRight(), Palette.TextMain, () => Cycle(-1),
                0, 0, 112, new Color(0.06f, 0.08f, 0.16f, 0.85f));
            Ui.Anchor((RectTransform)prev.transform, new Vector2(0, 0.5f), 76, 0, 112, 112);
            var prevGlyph = prev.transform.Find("Glyph");
            if (prevGlyph != null) prevGlyph.localScale = new Vector3(-1, 1, 1);
            var next = Ui.MakeIconButton(previewArea, "Next", Art.IconArrowRight(), Palette.TextMain, () => Cycle(1),
                0, 0, 112, new Color(0.06f, 0.08f, 0.16f, 0.85f));
            Ui.Anchor((RectTransform)next.transform, new Vector2(1, 0.5f), -76, 0, 112, 112);

            _name = Ui.MakeLabel(panel, "", 44, Palette.TextMain, TextAnchor.MiddleCenter, 0, 0, 760, 64);
            Ui.Bottom(_name.rectTransform, 526, 760, 64);
            _blurb = Ui.MakeLabel(panel, "", 27, Palette.TextDim, TextAnchor.MiddleCenter, 0, 0, 740, 84, false, true);
            Ui.Bottom(_blurb.rectTransform, 432, 740, 84);
            _price = Ui.MakeLabel(panel, "", 32, Palette.Gold, TextAnchor.MiddleCenter, 0, 0, 760, 66, true, true);
            Ui.Bottom(_price.rectTransform, 350, 760, 66);
            _progressBar = Ui.MakeBar(panel, 0, 0, 560, 16, new Color(0.1f, 0.12f, 0.2f, 0.9f), Palette.Magenta);
            Ui.Bottom((RectTransform)_progressBar.transform.parent, 324, 560, 16);
            _progressLabel = Ui.MakeLabel(panel, "", 27, Palette.Magenta, TextAnchor.MiddleCenter, 0, 0, 760, 50);
            Ui.Bottom(_progressLabel.rectTransform, 266, 760, 50);
            _action = Ui.MakeButton(panel, "SELECT", 40, new Color(0.04f, 0.26f, 0.30f, 0.96f), Palette.Cyan,
                OnAction, 0, 0, 640, 124, 1f);
            Ui.Bottom((RectTransform)_action.transform, 130, 640, 124);
            _adProgressButton = Ui.MakeButton(panel, "WATCH AD", 34, new Color(0.22f, 0.06f, 0.18f, 0.95f), Palette.Magenta,
                OnAdProgress, 0, 0, 640, 124, 1f, Palette.Magenta, Art.IconVideo());
            Ui.Bottom((RectTransform)_adProgressButton.transform, 130, 640, 124);
            _adReason = Ui.MakeLabel(panel, "", 22, Palette.TextDim, TextAnchor.MiddleCenter, 0, 0, 760, 32);
            Ui.Bottom(_adReason.rectTransform, 96, 760, 32);
            var back = Ui.MakeButton(panel, "BACK", 32, Palette.PanelFillSoft, Palette.TextDim, () => Hide(), 0, 0, 300, 88, 0.35f);
            Ui.Bottom((RectTransform)back.transform, 8, 300, 88);
        }

        public override void Refresh()
        {
            var save = GameApp.I.Save;
            _index = Mathf.Clamp(_index, 0, Skins.All.Length - 1);
            var skin = Skins.All[_index];
            _coins.text = save.coins.ToString("N0");
            _name.text = skin.name;
            _blurb.text = skin.blurb;
            _preview.sprite = Art.Car(skin);
            _glow.color = new Color(skin.glow.r, skin.glow.g, skin.glow.b, 0.26f);
            bool owned = save.Owns(skin.id), selected = save.selectedSkin == skin.id;
            bool adUnlock = skin.adUnlock && !owned;
            _action.gameObject.SetActive(!adUnlock);
            _adProgressButton.gameObject.SetActive(adUnlock);
            _progressBar.transform.parent.gameObject.SetActive(adUnlock);
            _progressLabel.gameObject.SetActive(adUnlock);
            if (adUnlock)
            {
                _progressLabel.text = Mathf.Min(save.adSkinProgress, Tuning.SkinAdCount) + " / " + Tuning.SkinAdCount + " VIDEOS  -  PROGRESS IS SAVED";
                Ui.SetBar(_progressBar, save.adSkinProgress / (float)Tuning.SkinAdCount);
                _price.text = "OPTIONAL AD UNLOCK";
                _price.color = Palette.Magenta;
            }
            else
            {
                var label = _action.GetComponentInChildren<Text>();
                label.text = owned ? (selected ? "SELECTED" : "SELECT") : "BUY  " + skin.price.ToString("N0");
                _action.interactable = owned ? !selected : save.coins >= skin.price;
                if (owned) _price.text = selected ? "IN USE" : "OWNED";
                else if (save.coins >= skin.price) _price.text = skin.price.ToString("N0") + " COINS";
                else _price.text = (skin.price - save.coins).ToString("N0") + " MORE COINS\nEarn coins in runs, daily rewards and missions.";
                _price.color = owned ? Palette.Cyan : save.coins >= skin.price ? Palette.Gold : Palette.TextDim;
            }
            RefreshAdState();
        }
        public void RefreshAdState()
        {
            if (_adProgressButton == null || GameApp.I == null || GameApp.I.Ads == null) return;
            bool visible = _adProgressButton.gameObject.activeSelf;
            bool available = visible && GameApp.I.Ads.Available(AdPlacement.SkinUnlock);
            _adProgressButton.interactable = available;
            _adReason.text = visible && !available ? GameApp.I.Ads.UnavailableReason(AdPlacement.SkinUnlock) : "";
        }
        void Cycle(int dir)
        {
            _index = (_index + dir + Skins.All.Length) % Skins.All.Length;
            Refresh();
            Hub.Pop(_preview.transform, 0.9f, 0.16f);
            Analytics.Event("skin_preview", "skin=" + Skins.All[_index].id);
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
            else Hub.Toast.Show("NOT ENOUGH COINS", Palette.TextDim);
            Refresh();
        }
        void OnAdProgress()
        {
            var app = GameApp.I;
            if (!app.Ads.Available(AdPlacement.SkinUnlock))
            {
                Hub.Toast.Show(app.Ads.UnavailableReason(AdPlacement.SkinUnlock), Palette.TextDim);
                return;
            }
            app.Ads.Show(AdPlacement.SkinUnlock, earned =>
            {
                if (Root == null) return;
                Refresh();
                if (earned) Hub.Toast.Show(app.Save.Owns("ghost") ? "CHROME GHOST UNLOCKED!" : "PROGRESS SAVED", Palette.Magenta);
            }, showId =>
            {
                Progression.AddAdSkinProgress(app.Save, "ad:" + showId + ":skin");
                app.SaveNow();
            });
        }
        public void OpenOn(string skinId) { _index = Skins.IndexOf(skinId); }
    }

    public class DailyScreen : UiScreen
    {
        readonly Image[] _cellFills = new Image[7];
        readonly Text[] _cellLabels = new Text[7];
        Button _claim, _adButton;
        Text _streak, _adLabel, _adReason;
        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0, 0, 0, 0.74f), true);
            Ui.Stretch(dim.rectTransform);
            var panel = Ui.FitPanel(Root, "Panel", 920, 1120, Palette.Gold);
            var title = Ui.MakeLabel(panel, "DAILY REWARD", 56, Palette.TextMain, TextAnchor.MiddleCenter, 0, 0, 800, 84);
            Ui.Top(title.rectTransform, 36, 800, 84);
            _streak = Ui.MakeLabel(panel, "", 28, Palette.Gold, TextAnchor.MiddleCenter, 0, 0, 820, 64);
            Ui.Top(_streak.rectTransform, 132, 820, 64);
            var days = Ui.Rect("Days", panel);
            days.anchorMin = new Vector2(0, 1); days.anchorMax = Vector2.one;
            days.pivot = new Vector2(0.5f, 1);
            days.offsetMin = new Vector2(32, -400); days.offsetMax = new Vector2(-32, -224);
            var layout = days.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            for (int i = 0; i < 7; i++)
            {
                var cell = Ui.MakePanel(days, "Day" + (i + 1), 114, 176, new Color(0.07f, 0.08f, 0.16f, 0.9f), Palette.Gold, 0.25f);
                _cellFills[i] = cell.GetComponent<Image>();
                Ui.MakeLabel(cell, "D" + (i + 1), 23, Palette.TextDim, TextAnchor.MiddleCenter, 0, 57, 100, 36);
                var gift = Ui.AddImage(cell, "Gift", Art.IconGift(), Palette.Gold);
                Ui.Place(gift.rectTransform, 0, 7, 48, 48);
                _cellLabels[i] = Ui.MakeLabel(cell, Progression.DailyRewards[i].ToString(), 25, Palette.TextMain, TextAnchor.MiddleCenter, 0, -49, 100, 42);
            }
            _claim = Ui.MakeButton(panel, "CLAIM", 42, new Color(0.28f, 0.22f, 0.02f, 0.96f), Palette.Gold,
                () => GameApp.I.ClaimDaily(), 0, 0, 620, 130, 1f);
            Ui.Bottom((RectTransform)_claim.transform, 410, 620, 130);
            _adButton = Ui.MakeButton(panel, "WATCH AD", 32, new Color(0.22f, 0.06f, 0.18f, 0.95f), Palette.Magenta,
                () => GameApp.I.WatchDailyAd(), 0, 0, 620, 112, 0.9f, Palette.Magenta, Art.IconVideo());
            Ui.Bottom((RectTransform)_adButton.transform, 264, 620, 112);
            _adLabel = _adButton.GetComponentInChildren<Text>();
            _adReason = Ui.MakeLabel(panel, "", 23, Palette.TextDim, TextAnchor.MiddleCenter, 0, 0, 800, 36);
            Ui.Bottom(_adReason.rectTransform, 214, 800, 36);
            var hint = Ui.MakeLabel(panel, "Your seven-day progress is kept when you miss a day.\nAfter day 7, a new cycle starts the next day.",
                25, Palette.TextDim, TextAnchor.MiddleCenter, 0, 0, 800, 90, false, true);
            Ui.Bottom(hint.rectTransform, 114, 800, 90);
            var back = Ui.MakeButton(panel, "BACK", 32, Palette.PanelFillSoft, Palette.TextDim, () => Hide(), 0, 0, 300, 90, 0.35f);
            Ui.Bottom((RectTransform)back.transform, 16, 300, 90);
        }
        public override void Refresh()
        {
            var save = GameApp.I.Save;
            int nextDay = Progression.NextDailyDay(save);
            bool canClaim = Progression.CanClaimDaily(save);
            _streak.text = canClaim ? "DAY " + nextDay + " OF 7 READY" : "COLLECTED TODAY - COME BACK TOMORROW";
            for (int i = 0; i < 7; i++)
            {
                int day = i + 1;
                bool isNext = canClaim && day == nextDay;
                bool collected = canClaim ? day < nextDay : day <= save.dailyStreak;
                _cellFills[i].color = isNext ? new Color(0.30f, 0.24f, 0.04f, 0.95f)
                    : collected ? new Color(0.06f, 0.14f, 0.16f, 0.9f) : new Color(0.07f, 0.08f, 0.16f, 0.9f);
                _cellLabels[i].color = isNext ? Palette.Gold : collected ? Palette.Cyan : Palette.TextDim;
            }
            _claim.GetComponentInChildren<Text>().text = canClaim ? "CLAIM  " + Progression.DailyRewardForDay(nextDay) : "CLAIMED";
            _claim.interactable = canClaim;
            RefreshAdState();
        }
        public void RefreshAdState()
        {
            if (_adButton == null || GameApp.I == null || GameApp.I.Ads == null) return;
            int left = Progression.DailyAdsLeft(GameApp.I.Save);
            bool available = left > 0 && GameApp.I.Ads.Available(AdPlacement.DailyBonus);
            _adButton.interactable = available;
            _adLabel.text = left > 0 ? "WATCH AD  +" + Tuning.DailyAdCoins + "  (" + left + " LEFT)" : "NO ADS LEFT TODAY";
            _adReason.text = left > 0 && !available ? GameApp.I.Ads.UnavailableReason(AdPlacement.DailyBonus) : "";
        }
    }

    public class QuestsScreen : UiScreen
    {
        class Row
        {
            public RectTransform root;
            public Text title, hint, progress, reward, claimLabel;
            public Image bar;
            public Button claim;
        }
        readonly Row[] _rows = new Row[3];
        Text _header, _coins;
        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0, 0, 0, 0.74f), true);
            Ui.Stretch(dim.rectTransform);
            var panel = Ui.FitPanel(Root, "Panel", 900, 1440, Palette.Violet);
            var title = Ui.MakeLabel(panel, "MISSIONS", 58, Palette.TextMain, TextAnchor.MiddleCenter, 0, 0, 800, 84);
            Ui.Top(title.rectTransform, 28, 800, 84);
            RectTransform wallet;
            _coins = Ui.Wallet(panel, out wallet);
            Ui.Top(wallet, 128, 300, 84);
            _header = Ui.MakeLabel(panel, "", 26, Palette.TextDim, TextAnchor.MiddleCenter, 0, 0, 800, 60);
            Ui.Top(_header.rectTransform, 226, 800, 60);
            var content = Ui.VerticalScroll(panel, "MissionList", 32, 132, 32, 306);
            for (int i = 0; i < 3; i++)
            {
                var row = new Row();
                row.root = Ui.MakePanel(content, "Quest" + i, 800, 300, new Color(0.06f, 0.07f, 0.14f, 0.92f), Palette.Violet, 0.25f);
                var element = row.root.gameObject.AddComponent<LayoutElement>();
                element.minHeight = element.preferredHeight = 300;
                row.title = Ui.MakeLabel(row.root, "", 32, Palette.TextMain, TextAnchor.MiddleLeft, 0, 0, 720, 56);
                HorizontalLabel(row.title.rectTransform, 24, 24, 20, 56);
                row.hint = Ui.MakeLabel(row.root, "", 24, Palette.TextDim, TextAnchor.MiddleLeft, 0, 0, 720, 64, false, true);
                HorizontalLabel(row.hint.rectTransform, 24, 24, 80, 64);
                row.bar = Ui.MakeBar(row.root, 0, 0, 720, 14, new Color(0.1f, 0.12f, 0.2f, 0.9f), Palette.Violet);
                HorizontalLabel((RectTransform)row.bar.transform.parent, 24, 24, 160, 14);
                row.progress = Ui.MakeLabel(row.root, "", 26, Palette.TextDim, TextAnchor.MiddleLeft, -200, -56, 290, 40);
                row.reward = Ui.MakeLabel(row.root, "", 25, Palette.Gold, TextAnchor.MiddleLeft, -200, -106, 290, 40);
                row.claim = Ui.MakeButton(row.root, "CLAIM", 27, new Color(0.26f, 0.18f, 0.02f, 0.96f), Palette.Gold,
                    MakeClaimHandler(i), 0, 0, 250, 96, 0.8f);
                Ui.Anchor((RectTransform)row.claim.transform, new Vector2(1, 0), -155, 68, 250, 96);
                row.claimLabel = row.claim.GetComponentInChildren<Text>();
                _rows[i] = row;
            }
            var back = Ui.MakeButton(panel, "BACK", 32, Palette.PanelFillSoft, Palette.TextDim, () => Hide(), 0, 0, 300, 96, 0.35f);
            Ui.Bottom((RectTransform)back.transform, 18, 300, 96);
        }
        static void HorizontalLabel(RectTransform rt, float left, float right, float top, float height)
        {
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(left, -top - height); rt.offsetMax = new Vector2(-right, -top);
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
                if (coins > 0) Hub.Toast.Show("+" + coins + " COINS", Palette.Gold);
                Refresh();
            };
        }
        public override void Refresh()
        {
            var save = GameApp.I.Save;
            Progression.EnsureQuests(save);
            _coins.text = save.coins.ToString("N0");
            int done = 0;
            for (int i = 0; i < _rows.Length; i++)
            {
                var row = _rows[i];
                row.root.gameObject.SetActive(i < save.quests.Count);
                if (i >= save.quests.Count) continue;
                var q = save.quests[i];
                var def = Progression.QuestById(q.id);
                row.title.text = def.title;
                row.hint.text = def.hint;
                row.progress.text = Mathf.Min(q.progress, def.target) + " / " + def.target;
                Ui.SetBar(row.bar, q.progress / (float)def.target);
                row.reward.text = "+" + def.reward + " COINS";
                bool ready = !q.claimed && q.progress >= def.target;
                if (ready) done++;
                row.claim.interactable = ready;
                row.claimLabel.text = q.claimed ? "CLAIMED" : ready ? "CLAIM" : "IN PROGRESS";
                row.title.color = q.claimed ? Palette.TextDim : Palette.TextMain;
            }
            _header.text = done > 0 ? done + " MISSION(S) READY TO CLAIM" : "New missions every day at midnight.";
            _header.color = done > 0 ? Palette.Gold : Palette.TextDim;
        }
    }
}

