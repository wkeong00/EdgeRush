using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EdgeRush
{
    public enum GameState
    {
        Boot,
        Menu,
        Tutorial,
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
        public string RunId { get; private set; }
        public bool Suspended => _applicationPaused || _applicationUnfocused;
        TutorialSession _tutorial;
        bool _tutorialReplay, _pausedFromTutorial;
        bool _applicationPaused, _applicationUnfocused;
        bool _reviveRewarded;
        float _runStartedAt;

        float _countdownElapsed;
        int _countdownShown = -1;
        System.Action _countdownDone;
        float _crashTimer;
        int _coinsEarnedThisRun;
        bool _newBestThisRun;

        // input
        readonly GameplayGesture _gesture = new GameplayGesture();
        readonly System.Collections.Generic.List<RaycastResult> _uiHits = new System.Collections.Generic.List<RaycastResult>();
        PointerEventData _pointerEvent;
        EventSystem _gestureEventSystem;
        bool _suppressTouchUntilReleased;
        bool _inputEnabled;

        public bool CanOfferRevive =>
            Run != null && !Run.Revived &&
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

            bool verification = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-edgeRushVerification") >= 0;
            Saver = verification
                ? new SaveSystem(System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/VerificationSave")), "")
                : new SaveSystem();
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
            if (Object.FindAnyObjectByType<AudioListener>() == null) camGo.AddComponent<AudioListener>();
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
        public void StartTutorial(bool replay = false)
        {
            if (Ads != null && Ads.IsShowing) return;
            StopTutorial();
            Run?.Stop();
            Run = null;
            _tutorialReplay = replay;
            ResetGameplayInput();
            UI.CloseAllMenus();
            UI.Menu.Hide(true);
            UI.Hud.Show(true);
            UI.Hud.ShowRunUi(true);
            UI.Hud.SetScore(0);
            UI.Hud.SetCoins(0);
            UI.Hud.SetCombo(1, 0f);
            Player.Setup(Skins.ById(Save.selectedSkin), false);
            Player.gameObject.SetActive(true);
            _tutorial = new TutorialSession(Player, Obstacles, Road);
            _tutorial.StepChanged += PresentTutorialStep;
            _tutorial.Completed += CompleteTutorial;
            _tutorial.Retried += reason => UI.Toast.Show(reason, Palette.Cyan);
            State = GameState.Tutorial;
            _inputEnabled = true;
            Audio.SetEngine(true, 0f);
            Analytics.Event("tutorial_start", "version=" + TutorialSession.Version + " replay=" + replay);
            _tutorial.Start();
        }

        void PresentTutorialStep(int step)
        {
            string key = step == 0 ? "left" : step == 1 ? "right" : "near_miss";
            UI.ShowTutorialStep(Strings.Get("tutorial." + key + ".title"), Strings.Get("tutorial." + key + ".hint"));
            Analytics.Event("tutorial_step", "step=" + key);
        }

        void CompleteTutorial()
        {
            Save.tutorialVersion = TutorialSession.Version;
            Save.tutorialCompleted = true;
            SaveNow();
            Analytics.Event("tutorial_complete", "version=" + TutorialSession.Version);
            bool replay = _tutorialReplay;
            StopTutorial();
            State = GameState.Menu;
            if (replay) EnterMenu();
            else StartRun();
        }

        public void SkipTutorial()
        {
            if (_tutorial == null) return;
            Save.tutorialVersion = TutorialSession.Version;
            Save.tutorialCompleted = true;
            SaveNow();
            Analytics.Event("tutorial_skip", "step=" + _tutorial.Step);
            bool replay = _tutorialReplay;
            StopTutorial();
            State = GameState.Menu;
            if (replay) EnterMenu();
            else StartRun();
        }

        void StopTutorial()
        {
            _tutorial?.Stop();
            _tutorial = null;
            UI?.HideTutorial();
        }

        public void EnterMenu()
        {
            StopTutorial();
            ResetGameplayInput();
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
            Player.gameObject.SetActive(false);
            Audio.SetEngine(false, 0f);
            UI.CloseAllMenus();
            UI.Menu.Show();
            Analytics.Event("menu_enter");
        }

        public void StartRun()
        {
            if (Ads != null && Ads.IsShowing) return;
            if (!Save.tutorialCompleted && State != GameState.Tutorial)
            {
                StartTutorial();
                return;
            }
            if (State == GameState.Countdown || State == GameState.Playing || State == GameState.Paused) return;

            StopTutorial();
            ResetGameplayInput();

            if (Run != null)
            {
                Run.Stop();
                Run = null;
            }
            UI.CloseAllMenus();
            UI.Menu.Hide(true);

            Ads.BeginRun();
            Save.totalRuns++;
            SessionRuns++;
            RunId = System.Guid.NewGuid().ToString("N");
            _runStartedAt = Time.realtimeSinceStartup;
            DoubleCoinsUsed = false;
            _coinsEarnedThisRun = 0;
            _newBestThisRun = false;
            _reviveRewarded = false;
            SaveNow();

            Player.Setup(Skins.ById(Save.selectedSkin), false);
            Player.ResetForRun();

            Player.gameObject.SetActive(true);
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
            Analytics.Event("run_identity", "runId=" + RunId + " tuning=" + Tuning.Version);

            BeginCountdown(1.05f, () =>
            {
                State = GameState.Playing;
                _inputEnabled = true;
                UI.Countdown.Hide();
            });
        }

        void BeginCountdown(float duration, System.Action onDone)
        {
            ResetGameplayInput();
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

            float dt = Time.unscaledDeltaTime;
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
            ResetGameplayInput();
            _crashTimer = 1.0f;

            Rig.Shake(Save.reducedEffects ? 0.125f : 0.5f, 0.4f);
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
            if (State != GameState.Playing && State != GameState.Tutorial) return;
            _pausedFromTutorial = State == GameState.Tutorial;
            State = GameState.Paused;
            _inputEnabled = false;
            ResetGameplayInput();
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
                State = _pausedFromTutorial ? GameState.Tutorial : GameState.Playing;
                _inputEnabled = true;
                UI.Countdown.Hide();
            });
        }

        public void RestartRun()
        {
            if (Ads != null && Ads.IsShowing) return;
            bool tutorial = _tutorial != null;
            StopTutorial();
            if (Run != null)
            {
                Run.Stop();
                Run = null;
            }
            Obstacles.ResetForRun();
            Road.ResetForRun();
            UI.CloseAllMenus();
            State = GameState.Menu;
            if (tutorial) StartTutorial(_tutorialReplay);
            else StartRun();
        }

        public void QuitToMenu()
        {
            if (Ads != null && Ads.IsShowing) return;
            if (Run != null && State != GameState.Results)
                Analytics.Event("run_quit", "runId=" + RunId + " activeSeconds=" + Run.Elapsed);
            SaveNow();
            EnterMenu();
        }

        // ------------------------------------------------------------- ad flows
        public void WatchReviveAd()
        {
            if (State != GameState.ReviveOffer || Ads.IsShowing) return;
            var run = Run;
            string runId = RunId;
            UI.Revive.Close();
            Ads.Show(AdPlacement.Revive, earned =>
            {
                if (Run != run || RunId != runId || State != GameState.ReviveOffer) return;
                if (_reviveRewarded && Run != null)
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
            }, showId =>
            {
                if (Run != run || RunId != runId || _reviveRewarded) return;
                if (!Progression.TryRecordReward(Save, "run:" + runId + ":revive", "ad_revive")) return;
                _reviveRewarded = true;
                SaveNow();
            });
        }

        public void DeclineRevive()
        {
            if (Ads != null && Ads.IsShowing) return;
            if (State != GameState.ReviveOffer) return;
            UI.Revive.Close();
            FinishRun();
        }

        public void WatchDoubleCoins()
        {
            if (State != GameState.Results || DoubleCoinsUsed || Ads.IsShowing) return;
            if (!Ads.Available(AdPlacement.DoubleCoins))
            {
                UI.Toast.Show("AD NOT READY - TRY AGAIN", Palette.TextDim);
                return;
            }
            string runId = RunId;
            int baseSettlement = _coinsEarnedThisRun;
            bool granted = false;
            Ads.Show(AdPlacement.DoubleCoins, earned =>
            {
                if (!granted || State != GameState.Results || RunId != runId) return;
                UI.Results.MarkDoubled(baseSettlement * 2);
                Audio.Play(Sfx.Buy, 0.9f);
                Haptics.Medium();
                UI.Toast.Show("+" + baseSettlement + " BONUS COINS", Palette.Gold);
            }, showId =>
            {
                granted = Progression.GrantCoins(Save, baseSettlement, "ad_double", "run:" + runId + ":double");
                if (!granted) return;
                if (RunId == runId) DoubleCoinsUsed = true;
                SaveNow();
            });
        }

        // --------------------------------------------------------------- finish
        void FinishRun()
        {
            if (State == GameState.Results) return;
            State = GameState.Results;
            _inputEnabled = false;
            ResetGameplayInput();

            var summary = Run != null ? Run.Summary() : new RunSummary();
            summary.runId = RunId;
            summary.realSeconds = Time.realtimeSinceStartup - _runStartedAt;
            LastSummary = summary;
            _coinsEarnedThisRun = summary.coins;

            Progression.GrantCoins(Save, summary.coins, "run_settlement", "run:" + RunId + ":settlement");
            _newBestThisRun = summary.score > Save.bestScore;
            if (_newBestThisRun) Save.bestScore = summary.score;
            Save.bestDistance = Mathf.Max(Save.bestDistance, summary.metres);
            Save.totalDistance += summary.metres;
            Save.totalNearMisses += summary.nearMisses;
            Save.bestComboLevel = Mathf.Max(Save.bestComboLevel, summary.comboLevel);
            Progression.ReportRun(Save, summary, RunId);
            SaveNow();

            Analytics.RunEnd(summary.score, summary.metres, summary.seconds, summary.nearMisses,
                             summary.comboLevel, summary.coins, summary.revived);

            Audio.SetEngine(false, 0f);
            UI.Hud.ShowRunUi(false);
            UI.Hud.Hide();

            bool canDouble = summary.coins > 0 && !DoubleCoinsUsed && Ads.RunAdsEligible;
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
            Rig.Shake(Save.reducedEffects ? 0.0175f : 0.07f, 0.09f);
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
            Progression.GrantCoins(Save, amount, reason, System.Guid.NewGuid().ToString("N"));
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
                UI.Daily.Refresh();
            }, showId =>
            {
                int coins = Progression.ClaimDailyAd(Save, "ad:" + showId + ":daily");
                if (coins > 0)
                {
                    SaveNow();
                    Audio.Play(Sfx.Buy, 0.8f);
                    UI.Toast.Show("+" + coins + " COINS", Palette.Gold);
                }
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
            if (_applicationPaused || _applicationUnfocused) return;
            if (Ads != null && Ads.IsShowing) return;
            switch (State)
            {
                case GameState.Countdown:
                    TickCountdown();
                    break;
                case GameState.Playing:
                    PollGameplayInput();
                    TickPlaying();
                    break;
                case GameState.Tutorial:
                    PollGameplayInput();
                    _tutorial?.Tick(Time.unscaledDeltaTime);
                    Fx.Tick(Time.unscaledDeltaTime);
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
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Player.Move(1);

            bool down = false, up = false, canceled = false;
            Vector2 pos = Vector2.zero;
            int pointerId = -1;

            if (Input.touchCount > 1)
            {
                ResetGameplayInput();
                _suppressTouchUntilReleased = true;
                return;
            }
            if (_suppressTouchUntilReleased)
            {
                if (Input.touchCount == 0) _suppressTouchUntilReleased = false;
                return;
            }

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                down = touch.phase == TouchPhase.Began;
                up = touch.phase == TouchPhase.Ended;
                canceled = touch.phase == TouchPhase.Canceled;
                pos = touch.position;
                pointerId = touch.fingerId;
            }
            else
            {
                down = Input.GetMouseButtonDown(0);
                up = Input.GetMouseButtonUp(0);
                pos = Input.mousePosition;
            }

            if (canceled || IsGameplayPointerOverUi(pos, pointerId))
            {
                _gesture.Cancel();
                return;
            }
            if (down) _gesture.Begin(pointerId, pos.x, pos.y, Time.unscaledTime);
            float threshold = Screen.width * 0.045f;
            int direction = up
                ? _gesture.End(pointerId, pos.x, pos.y, Time.unscaledTime, Screen.width, threshold)
                : _gesture.Move(pointerId, pos.x, pos.y, threshold);
            if (direction != 0) Player.Move(direction);
        }

        bool IsGameplayPointerOverUi(Vector2 position, int pointerId)
        {
            if (EventSystem.current == null) return false;
            if (_pointerEvent == null || _gestureEventSystem != EventSystem.current)
            {
                _pointerEvent = new PointerEventData(EventSystem.current);
                _gestureEventSystem = EventSystem.current;
            }
            _pointerEvent.Reset();
            _pointerEvent.position = position;
            _pointerEvent.pointerId = pointerId;
            _uiHits.Clear();
            EventSystem.current.RaycastAll(_pointerEvent, _uiHits);
            return _uiHits.Count > 0;
        }

        void ResetGameplayInput()
        {
            _gesture.Cancel();
            Player?.CancelBufferedMove();
        }

        void HandleBackButton()
        {
            if (Ads != null && Ads.IsShowing) return;
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            switch (State)
            {
                case GameState.Playing:
                case GameState.Tutorial:
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
            _applicationPaused = paused;
            ResetGameplayInput();
            if (paused)
            {
                SaveNow();
                if (State == GameState.Playing || State == GameState.Tutorial) PauseGame();
            }
        }

        void OnApplicationFocus(bool focused)
        {
            _applicationUnfocused = !focused;
            ResetGameplayInput();
            if (!focused && (State == GameState.Playing || State == GameState.Tutorial)) PauseGame();
        }

        void OnApplicationQuit()
        {
            SaveNow();
        }
    }
}





