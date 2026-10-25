using UnityEngine;
using UnityEngine.UI;

namespace EdgeRush
{
    // =============================================================== main menu
    public class MainMenuScreen : UiScreen
    {
        Text _best;
        Text _coins;
        Image _carPreview;
        RectTransform _carGlow;

        protected override void Construct()
        {
            var bg = Ui.AddFill(Root, "Backdrop", new Color(0.02f, 0.03f, 0.07f, 0.62f), true);
            Ui.Stretch(bg.rectTransform);

            // coins
            var coinPill = Ui.MakePanel(Root, "Coins", 300, 84, new Color(0.05f, 0.07f, 0.13f, 0.8f), Palette.Gold, 0.55f);
            Ui.Place(coinPill, -320, 860, 280, 84);
            var coinIcon = Ui.AddImage(coinPill, "CoinIcon", Art.Coin(), Color.white);
            Ui.Place(coinIcon.rectTransform, -110, 0, 54, 54);
            _coins = Ui.MakeLabel(coinPill, "0", 44, Palette.Gold, TextAnchor.MiddleLeft, -60, 0, 180, 60);

            // settings
            Ui.MakeIconButton(Root, "Settings", Art.IconGear(), Palette.TextDim,
                () => Hub.Settings.Show(), 422, 858, 90);

            // title
            var title = Ui.MakeLabel(Root, "EDGE RUSH", 150, Palette.Cyan, TextAnchor.MiddleCenter, 0, 470, 1000, 190);
            Ui.AddOutline(title, new Color(0.02f, 0.12f, 0.22f, 1f), 7f);
            var tag = Ui.MakeLabel(Root, "N E A R   M I S S   R A C E R", 34, Palette.Magenta, TextAnchor.MiddleCenter, 0, 350, 1000, 60);
            tag.raycastTarget = false;

            var bestPill = Ui.MakePanel(Root, "Best", 420, 78, new Color(0.05f, 0.07f, 0.13f, 0.72f), Palette.Violet, 0.5f);
            Ui.Place(bestPill, 0, 250, 420, 78);
            _best = Ui.MakeLabel(bestPill, "BEST 0", 40, Palette.TextMain, TextAnchor.MiddleCenter, 0, 0, 380, 60);

            // car preview with a soft glow behind it
            var glow = Ui.AddImage(Root, "CarGlow", Art.Glow(), new Color(0.3f, 0.9f, 1f, 0.30f));
            Ui.Place(glow.rectTransform, 0, 25, 480, 480);
            glow.raycastTarget = false;
            _carGlow = glow.rectTransform;

            var car = Ui.AddImage(Root, "CarPreview", Art.Car(Skins.All[0]), Color.white);
            Ui.Place(car.rectTransform, 0, 25, 240, 360);
            car.raycastTarget = false;
            car.preserveAspect = true;
            _carPreview = car;

            Ui.MakeButton(Root, "PLAY", 64, new Color(0.04f, 0.28f, 0.32f, 0.96f), Palette.Cyan,
                () => GameApp.I.StartRun(), 0, -250, 640, 180, 1f);

            Ui.MakeButton(Root, "GARAGE", 32, Palette.PanelFillSoft, Palette.TextMain,
                () => Hub.Garage.Show(), -290, -560, 280, 150, 0.55f, Palette.Cyan, Art.IconCar());
            Ui.MakeButton(Root, "DAILY", 32, Palette.PanelFillSoft, Palette.TextMain,
                () => Hub.Daily.Show(), 0, -560, 280, 150, 0.55f, Palette.Gold, Art.IconGift());
            Ui.MakeButton(Root, "MISSIONS", 32, Palette.PanelFillSoft, Palette.TextMain,
                () => Hub.Quests.Show(), 290, -560, 280, 150, 0.55f, Palette.Violet, Art.IconList());

            Ui.MakeLabel(Root, "Tap left / right or swipe to change lanes", 26, Palette.TextDim,
                TextAnchor.MiddleCenter, 0, -740, 900, 50);
        }

        public override void Refresh()
        {
            var save = GameApp.I.Save;
            _best.text = "BEST " + save.bestScore.ToString("N0");
            _coins.text = save.coins.ToString();
            var skin = Skins.ById(save.selectedSkin);
            _carPreview.sprite = Art.Car(skin);
            _carGlow.GetComponent<Image>().color = new Color(skin.glow.r, skin.glow.g, skin.glow.b, 0.26f);
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            Hub.Pop(_carPreview.transform, 0.86f, 0.28f);
        }
    }

    // ================================================================ settings
    public class SettingsScreen : UiScreen
    {
        Text _musicValue, _sfxValue, _hapticsValue;
        Button _musicBtn, _sfxBtn, _hapticsBtn;

        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.72f), true);
            Ui.Stretch(dim.rectTransform);

            var panel = Ui.MakePanel(Root, "Panel", 820, 1120, Palette.PanelFill, Palette.Violet, 0.9f);
            Ui.Place(panel, 0, 0, 820, 1120);

            Ui.MakeLabel(panel, "SETTINGS", 64, Palette.TextMain, TextAnchor.MiddleCenter, 0, 450, 700, 90);

            _musicBtn = MakeToggle(panel, "MUSIC", 300, out _musicValue, () =>
            {
                var s = GameApp.I.Save;
                s.musicOn = !s.musicOn;
                GameApp.I.SetMusic(s.musicOn);
                Refresh();
            });

            _sfxBtn = MakeToggle(panel, "SOUND EFFECTS", 170, out _sfxValue, () =>
            {
                var s = GameApp.I.Save;
                s.sfxOn = !s.sfxOn;
                GameApp.I.SetSfx(s.sfxOn);
                Refresh();
            });

            _hapticsBtn = MakeToggle(panel, "VIBRATION", 40, out _hapticsValue, () =>
            {
                var s = GameApp.I.Save;
                s.hapticsOn = !s.hapticsOn;
                GameApp.I.SetHaptics(s.hapticsOn);
                Refresh();
            });

            Ui.MakeButton(panel, "PRIVACY POLICY", 30, Palette.PanelFillSoft, Palette.TextDim, () =>
            {
                Application.OpenURL(BuildInfo.PrivacyPolicyUrl);
            }, 0, -140, 620, 100, 0.35f);

            Ui.MakeButton(panel, "ABOUT", 30, Palette.PanelFillSoft, Palette.TextDim, () =>
            {
                Hub.Toast.Show(BuildInfo.GameName + " " + BuildInfo.Version + "  -  made with Unity", Palette.Cyan);
            }, 0, -280, 620, 100, 0.35f);

            Ui.MakeLabel(panel, BuildInfo.GameName + "  v" + BuildInfo.Version + "\n" +
                               "Rewarded ads keep this game free.",
                24, Palette.TextDim, TextAnchor.MiddleCenter, 0, -410, 700, 90);

            Ui.MakeButton(panel, "BACK", 36, Palette.PanelFillSoft, Palette.TextMain,
                () => Hide(), 0, -500, 300, 90, 0.5f);
        }

        Button MakeToggle(Transform parent, string label, float y, out Text value, UnityEngine.Events.UnityAction onClick)
        {
            var row = Ui.MakePanel(parent, "Row_" + label, 660, 116, new Color(0.06f, 0.08f, 0.15f, 0.9f), Palette.Violet, 0.25f);
            Ui.Place(row, 0, y, 660, 116);
            Ui.MakeLabel(row, label, 38, Palette.TextMain, TextAnchor.MiddleLeft, -110, 0, 420, 80);
            var btn = Ui.MakeButton(row, "ON", 32, new Color(0.05f, 0.20f, 0.24f, 0.95f), Palette.Cyan, onClick,
                210, 0, 180, 78, 0.7f);
            value = btn.GetComponentInChildren<Text>();
            return btn;
        }

        public override void Refresh()
        {
            var s = GameApp.I.Save;
            _musicValue.text = s.musicOn ? "ON" : "OFF";
            _sfxValue.text = s.sfxOn ? "ON" : "OFF";
            _hapticsValue.text = s.hapticsOn ? "ON" : "OFF";
        }
    }
}


