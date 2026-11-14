using UnityEngine;
using UnityEngine.UI;

namespace EdgeRush
{
    public class MainMenuScreen : UiScreen
    {
        Text _best, _coins;
        Image _carPreview;
        RectTransform _carGlow;

        protected override void Construct()
        {
            var bg = Ui.AddFill(Root, "Backdrop", new Color(0.02f, 0.03f, 0.07f, 0.62f), true);
            Ui.Stretch(bg.rectTransform);
            RectTransform wallet;
            _coins = Ui.Wallet(Root, out wallet);
            Ui.Anchor(wallet, new Vector2(0, 1), 198, -94, 300, 84);
            var settings = Ui.MakeIconButton(Root, "Settings", Art.IconGear(), Palette.TextDim,
                () => Hub.Settings.Show(), 0, 0, 104);
            Ui.Anchor((RectTransform)settings.transform, Vector2.one, -100, -94, 104, 104);

            var content = Ui.Rect("Content", Root);
            Ui.Stretch(content, 48, 300, 48, 180);
            var title = Ui.MakeLabel(content, "EDGE RUSH", 138, Palette.Cyan, TextAnchor.MiddleCenter, 0, 0, 900, 170);
            Ui.Top(title.rectTransform, 16, 900, 170);
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 80;
            title.resizeTextMaxSize = 138;
            Ui.AddOutline(title, new Color(0.02f, 0.12f, 0.22f, 1f), 6f);
            var tag = Ui.MakeLabel(content, Strings.Get("menu.tagline"), 32, Palette.Magenta, TextAnchor.MiddleCenter, 0, 0, 840, 64);
            Ui.Top(tag.rectTransform, 182, 840, 64);
            var bestPill = Ui.MakePanel(content, "Best", 440, 78, new Color(0.05f, 0.07f, 0.13f, 0.72f), Palette.Violet, 0.5f);
            Ui.Top(bestPill, 270, 440, 78);
            _best = Ui.MakeLabel(bestPill, "BEST 0", 38, Palette.TextMain, TextAnchor.MiddleCenter, 0, 0, 400, 64);

            var glow = Ui.AddImage(content, "CarGlow", Art.Glow(), new Color(0.3f, 0.9f, 1f, 0.30f));
            Ui.Place(glow.rectTransform, 0, -50, 480, 480);
            _carGlow = glow.rectTransform;
            _carPreview = Ui.AddImage(content, "CarPreview", Art.Car(Skins.All[0]), Color.white);
            Ui.Place(_carPreview.rectTransform, 0, -50, 240, 360);
            _carPreview.preserveAspect = true;
            var play = Ui.MakeButton(content, Strings.Get("menu.play"), 62, new Color(0.04f, 0.28f, 0.32f, 0.96f), Palette.Cyan,
                () => GameApp.I.StartRun(), 0, 0, 640, 160, 1f);
            Ui.Bottom((RectTransform)play.transform, 20, 640, 160);

            var nav = Ui.Rect("Navigation", Root);
            nav.anchorMin = new Vector2(0, 0);
            nav.anchorMax = new Vector2(1, 0);
            nav.pivot = new Vector2(0.5f, 0);
            nav.offsetMin = new Vector2(48, 96);
            nav.offsetMax = new Vector2(-48, 240);
            var layout = nav.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            Ui.MakeButton(nav, "GARAGE", 30, Palette.PanelFillSoft, Palette.TextMain,
                () => Hub.Garage.Show(), 0, 0, 280, 144, 0.55f, Palette.Cyan);
            Ui.MakeButton(nav, "DAILY", 30, Palette.PanelFillSoft, Palette.TextMain,
                () => Hub.Daily.Show(), 0, 0, 280, 144, 0.55f, Palette.Gold);
            Ui.MakeButton(nav, "MISSIONS", 30, Palette.PanelFillSoft, Palette.TextMain,
                () => Hub.Quests.Show(), 0, 0, 280, 144, 0.55f, Palette.Violet);
            var hint = Ui.MakeLabel(Root, Strings.Get("controls.hint"), 25, Palette.TextDim, TextAnchor.MiddleCenter, 0, 0, 850, 50);
            Ui.Bottom(hint.rectTransform, 24, 850, 50);
        }

        public override void Refresh()
        {
            var save = GameApp.I.Save;
            _best.text = "BEST " + save.bestScore.ToString("N0");
            _coins.text = save.coins.ToString("N0");
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

    public class SettingsScreen : UiScreen
    {
        Text _musicValue, _sfxValue, _hapticsValue, _reducedValue;
        protected override void Construct()
        {
            var dim = Ui.AddFill(Root, "Dim", new Color(0f, 0f, 0f, 0.72f), true);
            Ui.Stretch(dim.rectTransform);
            var panel = Ui.FitPanel(Root, "Panel", 820, 1420, Palette.Violet);
            var title = Ui.MakeLabel(panel, "SETTINGS", 60, Palette.TextMain, TextAnchor.MiddleCenter, 0, 0, 700, 90);
            Ui.Top(title.rectTransform, 38, 700, 90);
            MakeToggle(panel, "MUSIC", 450, out _musicValue, () =>
            {
                var s = GameApp.I.Save; s.musicOn = !s.musicOn;
                GameApp.I.SetMusic(s.musicOn); Refresh();
            });
            MakeToggle(panel, "SOUND EFFECTS", 310, out _sfxValue, () =>
            {
                var s = GameApp.I.Save; s.sfxOn = !s.sfxOn;
                GameApp.I.SetSfx(s.sfxOn); Refresh();
            });
            MakeToggle(panel, "VIBRATION", 170, out _hapticsValue, () =>
            {
                var s = GameApp.I.Save; s.hapticsOn = !s.hapticsOn;
                GameApp.I.SetHaptics(s.hapticsOn); Refresh();
            });
            MakeToggle(panel, Strings.Get("settings.reduced_effects"), 30, out _reducedValue, () =>
            {
                var s = GameApp.I.Save; s.reducedEffects = !s.reducedEffects;
                GameApp.I.SaveNow(); Refresh();
            });
            Ui.MakeButton(panel, Strings.Get("settings.replay_tutorial"), 32, Palette.PanelFillSoft, Palette.Cyan,
                () => { Hide(true); GameApp.I.StartTutorial(true); }, 0, -125, 620, 104, 0.7f);
            Ui.MakeButton(panel, "PRIVACY POLICY", 30, Palette.PanelFillSoft, Palette.TextDim,
                () => Application.OpenURL(BuildInfo.PrivacyPolicyUrl), 0, -255, 620, 100, 0.35f);
            Ui.MakeButton(panel, "ABOUT", 30, Palette.PanelFillSoft, Palette.TextDim,
                () => Hub.Toast.Show(BuildInfo.GameName + " " + BuildInfo.Version + "  -  made with Unity", Palette.Cyan),
                0, -385, 620, 100, 0.35f);
            Ui.MakeLabel(panel, BuildInfo.GameName + "  v" + BuildInfo.Version + "\nOptional rewarded ads support this game.",
                24, Palette.TextDim, TextAnchor.MiddleCenter, 0, -505, 700, 90);
            var back = Ui.MakeButton(panel, "BACK", 34, Palette.PanelFillSoft, Palette.TextMain,
                () => Hide(), 0, 0, 300, 96, 0.5f);
            Ui.Bottom((RectTransform)back.transform, 28, 300, 96);
        }
        void MakeToggle(Transform parent, string label, float y, out Text value, UnityEngine.Events.UnityAction onClick)
        {
            var row = Ui.MakePanel(parent, "Row_" + label, 660, 116, new Color(0.06f, 0.08f, 0.15f, 0.9f), Palette.Violet, 0.25f);
            Ui.Place(row, 0, y, 660, 116);
            Ui.MakeLabel(row, label, 34, Palette.TextMain, TextAnchor.MiddleLeft, -110, 0, 400, 80);
            var btn = Ui.MakeButton(row, "ON", 32, new Color(0.05f, 0.20f, 0.24f, 0.95f), Palette.Cyan, onClick,
                210, 0, 180, 96, 0.7f);
            value = btn.GetComponentInChildren<Text>();
        }
        public override void Refresh()
        {
            var s = GameApp.I.Save;
            _musicValue.text = s.musicOn ? "ON" : "OFF";
            _sfxValue.text = s.sfxOn ? "ON" : "OFF";
            _hapticsValue.text = s.hapticsOn ? "ON" : "OFF";
            _reducedValue.text = s.reducedEffects ? "ON" : "OFF";
        }
    }
}

