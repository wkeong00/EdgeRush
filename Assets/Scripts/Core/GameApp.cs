using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EdgeRush
{
    public enum GameState
    {
        Boot,
        Menu,
        Countdown,
        Playing,
        Paused,
        Crashed,
        ReviveOffer,
        Results,
    }

    /// <summary>
    /// The hub: builds the world, owns the state machine, routes input and wires the systems
    /// together. Everything is created from code so the project has no scene to maintain.
    /// </summary>
    public class GameApp : MonoBehaviour
    {
        public static GameApp I { get; private set; }

        public SaveSystem Saver;
        public SaveData Save => Saver.Data;

        public Camera Cam;
        public CameraRig Rig;
        public RoadView Road;
        public PlayerCar Player;
        public ObstacleManager Obstacles;
        public FxManager Fx;
        public UiRoot UI;
        public AudioDirector Audio;
        public AdManager Ads;

        public GameState State { get; private set; } = GameState.Boot;
        public RunSession Run { get; private set; }
        public RunSummary LastSummary;
        public int SessionRuns;
        public bool DoubleCoinsUsed;

        float _countdownElapsed;
        int _countdownShown = -1;
        System.Action _countdownDone;
        float _crashTimer;
        int _coinsEarnedThisRun;
        bool _newBestThisRun;

        // input
        bool _pointerDown;
        Vector2 _downPos;
        float _downTime;
        bool _inputEnabled;

        public bool CanOfferRevive =>
            Run != null && !Run.Revived &&
            Save.totalRuns >= Tuning.ReviveAdOfferAfterRuns &&
            Ads != null && Ads.Available(AdPlacement.Revive);

        // ------------------------------------------------------------------ boot
        void Awake()
        {
            if (I != null)
            {
                Destroy(gameObject);
                return;
            }
            I = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;

            Saver = new SaveSystem();
            Saver.Load();
            Haptics.SetEnabled(Save.hapticsOn);

            BuildWorld();

            Analytics.Event("session_start", "sessions=" + Save.sessions);
            Analytics.Event("save_state", "coins=" + Save.coins + " best=" + Save.bestScore);
        }

        void BuildWorld()
        {
            Layout.Init(Screen.width / (float)Mathf.Max(1, Screen.height));

            // camera
            var camGo = new GameObject("MainCamera", typeof(Camera));
            camGo.transform.SetParent(transform, false);
            Cam = camGo.GetComponent<Camera>();
            Cam.orthographic = true;
            Cam.orthographicSize = Layout.OrthoSize;
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = Palette.Bg;
            Cam.nearClipPlane = 0.05f;
            Cam.farClipPlane = 100f;
            Cam.transform.localPosition = new Vector3(0f, 0f, -10f);
            Rig = gameObject.AddComponent<CameraRig>();
            Rig.Build(Cam);

            // audio
            Audio = gameObject.AddComponent<AudioDirector>();
            Audio.Init(Save.musicOn, Save.sfxOn);

            // fx
            Fx = gameObject.AddComponent<FxManager>();
            Fx.Build();

            // world
            var roadGo = new GameObject("RoadView");
            roadGo.transform.SetParent(transform, false);
            Road = roadGo.AddComponent<RoadView>();
            Road.Build(Cam);

            var obsGo = new GameObject("ObstacleManager");
            obsGo.transform.SetParent(transform, false);
            Obstacles = obsGo.AddComponent<ObstacleManager>();
            Obstacles.Build();

            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(transform, false);
            Player = playerGo.AddComponent<PlayerCar>();
            Player.Build();
            Player.Setup(Skins.ById(Save.selectedSkin), false);
            Player.ResetForRun();

            // ads
            Ads = gameObject.AddComponent<AdManager>();
            Ads.Init();

            // ui
            var uiGo = new GameObject("UI");
            uiGo.transform.SetParent(transform, false);
            UI = uiGo.AddComponent<UiRoot>();
            UI.Build();
        }

        void Start()
        {
            Audio.StartMusic();
            Audio.SetSpeedT(0f);
            EnterMenu();
            StartCoroutine(PrewarmArt());
        }

        /// <summary>
        /// Generates the remaining sprites (other skins, traffic variants, other themes) one per
        /// frame while the menu is up, so nothing is generated in the middle of a run.
        /// </summary>
        System.Collections.IEnumerator PrewarmArt()
        {
            yield return null;
            for (int i = 0; i < Skins.All.Length; i++)
            {
                Art.Car(Skins.All[i]);
                yield return null;
            }
            for (int i = 0; i < 3; i++) { Art.TrafficCar(i); yield return null; }
            for (int i = 0; i < 2; i++) { Art.Truck(i); yield return null; }
            Art.Barrier();
            yield return null;
            Art.Coin();
            yield return null;
            for (int i = 1; i < Palette.Themes.Length; i++)
            {
                Art.RoadTexture(Palette.Themes[i]);
                yield return null;
                Art.SpeedLines(Palette.Themes[i]);
                yield return null;
                Art.SkyGradient(Palette.Themes[i]);
                Art.Fog(Layout.OrthoSize * 0.55f, Palette.Themes[i]);
                yield return null;
            }
            Art.Decor(1);
            yield return null;
            Art.Decor(2);
            yield return null;
            Debug.Log("[EdgeRush] Art prewarm complete.");
        }

        // ----------------------------------------------------------- state flow
        public void EnterMenu()
        {
            State = GameState.Menu;
            _inputEnabled = false;
            if (Run != null)
            {
                Run.Stop();
                Run = null;
            }
            Obstacles.ResetForRun();
            Road.ResetForRun();
            Player.ResetForRun();
            Player.Setup(Skins.ById(Save.selectedSkin), false);
            Player.gameObject.SetActive(true);
            Audio.SetEngine(false, 0f);
            UI.CloseAllMenus();
            UI.Menu.Show();
            Analytics.Event("menu_enter");
        }

        public void StartRun()
        {
            if (State == GameState.Countdown || State == GameState.Playing || State == GameState.Paused) return;

            if (Run != null)
            {
                Run.Stop();
                Run = null;
            }
            UI.CloseAllMenus();
            UI.Menu.Hide(true);

            Save.totalRuns++;
            SessionRuns++;
            DoubleCoinsUsed = false;
            _coinsEarnedThisRun = 0;
            _newBestThisRun = false;

            Player.Setup(Skins.ById(Save.selectedSkin), false);
            Player.ResetForRun();

            Run = new RunSession(Player, Obstacles, Road, new System.Random(SessionRuns * 7919 + Save.totalRuns * 104729));
            Run.OnCoin += HandleCoin;
            Run.OnNearMiss += HandleNearMiss;
            Run.OnComboLevelUp += HandleComboLevelUp;
            Run.OnCrash += HandleCrash;
            Run.Start();

            UI.Hud.Show(true);
            UI.Hud.ShowRunUi(true);
            UI.Hud.SetScore(0);
            UI.Hud.SetCoins(0);
            UI.Hud.SetCombo(1, 0);

            Audio.SetEngine(true, 0f);
            Analytics.RunStart(Save.totalRuns);

            BeginCountdown(1.05f, () =>
            {
                State = GameState.Playing;
                _inputEnabled = true;
                UI.Countdown.Hide();
            });
        }

        void BeginCountdown(float duration, System.Action onDone)
        {
            State = GameState.Countdown;
            _countdownElapsed = 0f;
            _countdownShown = -1;
            _countdownDone = onDone;
            _inputEnabled = false;
            UI.Countdown.Show("3");
            Audio.Play(Sfx.Click, 0.3f);
        }

        void TickCountdown()
        {
            _countdownElapsed += Time.unscaledDeltaTime;
            int step = Mathf.FloorToInt(_countdownElapsed / 0.34f);
            if (step != _countdownShown && step <= 3)
            {
                _countdownShown = step;
                if (step < 3) UI.Countdown.Show((3 - step).ToString());
                else UI.Countdown.Show("GO!");
                if (step > 0) Audio.Play(Sfx.Click, 0.35f);
            }
            if (_countdownElapsed >= 1.02f + 0.0f)
            {
                var done = _countdownDone;
                _countdownDone = null;
                UI.Countdown.Hide();
                done?.Invoke();
            }
        }

        void TickPlaying()
        {
            if (Run == null) return;

            float dt = Time.deltaTime;
            Run.Tick(dt);

            UI.Hud.SetScore(Run.Score);
            UI.Hud.SetCoins(Run.Coins);
            UI.Hud.SetCombo(Run.ComboLevel, Run.ComboProgress);
            Audio.SetSpeedT(Run.SpeedT);
            Audio.SetEngine(true, Run.SpeedT);
            Rig.Tick(dt, Run.SpeedT, true);
            Fx.Tick(dt);

            if (Run.Crashed) BeginCrash();
        }

        void BeginCrash()
        {
            if (State == GameState.Crashed || State == GameState.Results) return;
            State = GameState.Crashed;
            _inputEnabled = false;
            _crashTimer = 1.0f;

            Rig.Shake(0.5f, 0.4f);
            UI.Flash.Flash(Color.white, 0.3f, 0.75f);
            Fx.Crash(Player.transform.position, Skins.ById(Save.selectedSkin).body);
            Audio.Play(Sfx.Crash, 0.9f);
            Audio.SetEngine(false, 0f);
            Haptics.Strong();
            Analytics.Event("crash", "m=" + Run.Metres + " score=" + Run.Score);
        }

        void TickCrash()
        {
            float dt = Time.deltaTime;
            _crashTimer -= Time.unscaledDeltaTime;
            Rig.Tick(dt, 0f, false);
            Fx.Tick(dt);

            if (_crashTimer > 0f) return;

            if (CanOfferRevive)
            {
                State = GameState.ReviveOffer;
                UI.Revive.Present(5f, Run.ComboLevel);
                Analytics.Event("revive_offer", "score=" + Run.Score);
            }
            else
            {
                FinishRun();
            }
        }

        public void PauseGame()
        {
            if (State != GameState.Playing) return;
            State = GameState.Paused;
            _inputEnabled = false;
            UI.Pause.Show();
            Audio.SetEngine(false, 0f);
            Analytics.Event("pause");
        }

        public void ResumeGame()
        {
            if (State != GameState.Paused) return;
            UI.Pause.Hide(true);
            BeginCountdown(1.0f, () =>
            {
                State = GameState.Playing;
                _inputEnabled = true;
                UI.Countdown.Hide();
            });
        }

        public void RestartRun()
        {
            if (Run != null)
            {
                Run.Stop();
                Run = null;
            }
            Obstacles.ResetForRun();
            Road.ResetForRun();
            UI.CloseAllMenus();
            State = GameState.Menu;
            StartRun();
        }

        public void QuitToMenu()
        {
            SaveNow();
            EnterMenu();
        }

        // ------------------------------------------------------------- ad flows
        public void WatchReviveAd()
        {
            if (State != GameState.ReviveOffer) return;
            UI.Revive.Close();
            Ads.Show(AdPlacement.Revive, earned =>
            {
                if (earned && Run != null)
                {
                    Run.ApplyRevive();
                    UI.Hud.ShowRunUi(true);
                    UI.Hud.SetCombo(Run.ComboLevel, 0f);
                    Audio.Play(Sfx.Revive, 0.9f);
                    Fx.Burst(Player.transform.position, Palette.Cyan, 22, 3f, 11f, 0.6f, 0.7f, true);
                    BeginCountdown(1.0f, () =>
                    {
                        State = GameState.Playing;
                        _inputEnabled = true;
                        UI.Countdown.Hide();
                    });
                }
                else
                {
                    FinishRun();
                }
            });
        }

        public void DeclineRevive()
        {
            if (State != GameState.ReviveOffer) return;
            UI.Revive.Close();
            FinishRun();
        }

        public void WatchDoubleCoins()
        {
            if (State != GameState.Results || DoubleCoinsUsed) return;
            if (!Ads.Available(AdPlacement.DoubleCoins))
            {
                UI.Toast.Show("AD NOT READY - TRY AGAIN", Palette.TextDim);
                return;
            }
            Ads.Show(AdPlacement.DoubleCoins, earned =>
            {
                if (!earned || State != GameState.Results) return;
                DoubleCoinsUsed = true;
                int bonus = Mathf.Max(_coinsEarnedThisRun, 1);
                Progression.AddCoins(Save, bonus);
                SaveNow();
                UI.Results.MarkDoubled(_coinsEarnedThisRun + bonus);
                Audio.Play(Sfx.Buy, 0.9f);
                Haptics.Medium();
                UI.Toast.Show("+" + bonus + " BONUS COINS", Palette.Gold);
            });
        }

        // --------------------------------------------------------------- finish
        void FinishRun()
        {
            if (State == GameState.Results) return;
            State = GameState.Results;
            _inputEnabled = false;

            var summary = Run != null ? Run.Summary() : new RunSummary();
            LastSummary = summary;
            _coinsEarnedThisRun = summary.coins;

            Progression.AddCoins(Save, summary.coins);
            _newBestThisRun = summary.score > Save.bestScore;
            if (_newBestThisRun) Save.bestScore = summary.score;
            Save.bestDistance = Mathf.Max(Save.bestDistance, summary.metres);
            Save.totalDistance += summary.metres;
            Save.totalNearMisses += summary.nearMisses;
            Save.bestComboLevel = Mathf.Max(Save.bestComboLevel, summary.comboLevel);
            Progression.ReportRun(Save, summary);
            SaveNow();

            Analytics.RunEnd(summary.score, summary.metres, summary.seconds, summary.nearMisses,
                             summary.comboLevel, summary.coins, summary.revived);

            Audio.SetEngine(false, 0f);
            UI.Hud.ShowRunUi(false);
            UI.Hud.Hide();

            bool canDouble = summary.coins > 0 && !DoubleCoinsUsed && Ads.Available(AdPlacement.DoubleCoins);
            UI.Results.Present(summary, Save.bestScore, _newBestThisRun, summary.coins, canDouble);
            UI.Results.Show();

            if (_newBestThisRun)
            {
                Audio.Play(Sfx.Record, 0.9f);
                UI.Flash.Flash(Palette.Gold, 0.5f, 0.35f);
                Haptics.Medium();
            }
        }

        // --------------------------------------------------------------- events
        void HandleCoin(int total)
        {
            Audio.Play(Sfx.Coin, 0.55f);
            Fx.CoinBurst(Player.transform.position);
            UI.SpawnFloating("+1", new Vector2(Player.transform.position.x * 40f, -150f), Palette.Gold, 40);
        }

        void HandleNearMiss(int total, int points)
        {
            if (Run == null) return;
            UI.Hud.PopClose(points, Run.ComboLevel);
            Audio.Play(Sfx.Whoosh, 0.7f);
            Haptics.Light();
            Rig.Shake(0.07f, 0.09f);
            Fx.Sparks(Player.transform.position + Vector3.up * 1.2f, Palette.Cyan, 10, 8f);
        }

        void HandleComboLevelUp()
        {
            Audio.Play(Sfx.Combo, 0.8f);
            Haptics.Medium();
            UI.Flash.Flash(Palette.Cyan, 0.25f, 0.2f);
        }

        void HandleCrash() => BeginCrash();

        // ------------------------------------------------------------ meta ops
        public void SaveNow() => Saver.Save();

        public void AddCoins(int amount, string reason)
        {
            if (amount == 0) return;
            Save.coins = Mathf.Max(0, Save.coins + amount);
            if (amount > 0) Save.totalCoinsEarned += amount;
            SaveNow();
        }

        public bool BuySkin(SkinDef skin)
        {
            if (!Progression.TryBuySkin(Save, skin)) return false;
            SaveNow();
            Player.Setup(skin, false);
            return true;
        }

        public void SelectSkin(SkinDef skin)
        {
            if (!Progression.SelectSkin(Save, skin)) return;
            SaveNow();
            Player.Setup(skin, false);
        }

        public void ClaimDaily()
        {
            int coins = Progression.ClaimDaily(Save);
            if (coins <= 0)
            {
                UI.Toast.Show("ALREADY CLAIMED TODAY", Palette.TextDim);
                return;
            }
            SaveNow();
            Audio.Play(Sfx.Buy, 0.8f);
            UI.Toast.Show("+" + coins + " COINS", Palette.Gold);
            UI.Daily.Refresh();
        }

        public void WatchDailyAd()
        {
            if (!Ads.Available(AdPlacement.DailyBonus))
            {
                UI.Toast.Show("AD NOT READY - TRY AGAIN", Palette.TextDim);
                return;
            }
            Ads.Show(AdPlacement.DailyBonus, earned =>
            {
                if (!earned) return;
                int coins = Progression.ClaimDailyAd(Save);
                if (coins > 0)
                {
                    SaveNow();
                    Audio.Play(Sfx.Buy, 0.8f);
                    UI.Toast.Show("+" + coins + " COINS", Palette.Gold);
                }
                UI.Daily.Refresh();
            });
        }

        public void SetMusic(bool on)
        {
            Save.musicOn = on;
            Audio.SetMusicEnabled(on);
            SaveNow();
        }

        public void SetSfx(bool on)
        {
            Save.sfxOn = on;
            Audio.SetSfxEnabled(on);
            SaveNow();
        }

        public void SetHaptics(bool on)
        {
            Save.hapticsOn = on;
            Haptics.SetEnabled(on);
            SaveNow();
        }

        // ---------------------------------------------------------------- update
        void Update()
        {
            switch (State)
            {
                case GameState.Countdown:
                    TickCountdown();
                    break;
                case GameState.Playing:
                    TickPlaying();
                    PollGameplayInput();
                    break;
                case GameState.Crashed:
                    TickCrash();
                    break;
                case GameState.Menu:
                    Road.Tick(Time.deltaTime, 3.5f, 0.18f);
                    Fx.Tick(Time.deltaTime);
                    Rig.Tick(Time.deltaTime, 0f, false);
                    break;
                case GameState.Results:
                    Fx.Tick(Time.deltaTime);
                    Rig.Tick(Time.deltaTime, 0f, false);
                    break;
            }

            HandleBackButton();
        }

        void PollGameplayInput()
        {
            if (!_inputEnabled) return;

            // keyboard for editor / desktop testing
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Player.Move(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Player.Move(1);

            bool down = false, up = false;
            Vector2 pos = Vector2.zero;
            bool overUi = false;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                down = touch.phase == TouchPhase.Began;
                up = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                pos = touch.position;
                if (EventSystem.current != null) overUi = EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
            else
            {
                down = Input.GetMouseButtonDown(0);
                up = Input.GetMouseButtonUp(0);
                pos = Input.mousePosition;
                if (EventSystem.current != null) overUi = EventSystem.current.IsPointerOverGameObject();
            }

            if (down && !overUi)
            {
                _pointerDown = true;
                _downPos = pos;
                _downTime = Time.unscaledTime;
            }
            else if (up && _pointerDown)
            {
                _pointerDown = false;
                if (overUi) return;

                float dx = pos.x - _downPos.x;
                float dy = pos.y - _downPos.y;
                float threshold = Screen.width * 0.045f;
                float held = Time.unscaledTime - _downTime;

                if (Mathf.Abs(dx) > threshold && Mathf.Abs(dx) > Mathf.Abs(dy))
                {
                    Player.Move(dx > 0f ? 1 : -1);
                }
                else if (held < 0.32f && Mathf.Abs(dx) < threshold * 1.5f)
                {
                    Player.Move(pos.x < Screen.width * 0.5f ? -1 : 1);
                }
            }
        }

        void HandleBackButton()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            switch (State)
            {
                case GameState.Playing:
                    PauseGame();
                    break;
                case GameState.Paused:
                    ResumeGame();
                    break;
                case GameState.ReviveOffer:
                    DeclineRevive();
                    break;
                case GameState.Results:
                    QuitToMenu();
                    break;
                case GameState.Crashed:
                    break;
                default:
                    if (UI.Settings.Visible) UI.Settings.Hide();
                    else if (UI.Garage.Visible) UI.Garage.Hide();
                    else if (UI.Daily.Visible) UI.Daily.Hide();
                    else if (UI.Quests.Visible) UI.Quests.Hide();
                    else if (UI.Pause.Visible) UI.Pause.Hide(true);
                    else Application.Quit();
                    break;
            }
        }

        // ------------------------------------------------------------- lifecycle
        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveNow();
                if (State == GameState.Playing) PauseGame();
            }
        }

        void OnApplicationQuit()
        {
            SaveNow();
        }
    }
}





