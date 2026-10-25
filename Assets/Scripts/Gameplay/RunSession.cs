using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>
    /// One run of the game: speed and density curves, traffic spawning, scoring, the near miss
    /// combo and the revive rules. Kept free of UI so it is easy to tune and test.
    /// </summary>
    public class RunSession
    {
        readonly PlayerCar _player;
        readonly ObstacleManager _obstacles;
        readonly RoadView _road;
        readonly System.Random _rnd;

        readonly List<RowSpec> _queue = new List<RowSpec>();
        readonly List<Obstacle> _tagged = new List<Obstacle>();

        Pattern _lastPattern;
        int _lastDifficulty = -1;
        float _rowTimer;
        float _slowTimer;
        float _reviveRamp;
        float _speedMult = 1f;
        float _comboTimer;
        float _decayTimer;
        int _coinScore;
        int _nearMissScore;
        bool _crashed;
        int _themeIndex;

        public float Elapsed { get; private set; }
        public float Distance { get; private set; }
        public float Speed { get; private set; }
        public float SpeedT { get; private set; }
        public int Score { get; private set; }
        public int Coins { get; private set; }
        public int NearMisses { get; private set; }
        public int ComboLevel { get; private set; } = 1;
        public int ComboChain { get; private set; }
        public bool Revived { get; private set; }
        public bool Crashed => _crashed;
        public float ComboProgress => Mathf.Clamp01(ComboChain / (float)Tuning.ComboStep);
        public float ComboDecayProgress => Mathf.Clamp01((_comboTimer - Tuning.ComboGraceSeconds) / (Tuning.ComboDecayEvery * (Tuning.ComboMax - 1)));
        public int Metres => Mathf.RoundToInt(Distance * Layout.MetresPerUnit);
        public float Seconds => Elapsed;

        public event Action<int> OnCoin;          // total coins
        public event Action<int, int> OnNearMiss; // total near misses, points scored
        public event Action OnCrash;
        public event Action OnComboLevelUp;

        public RunSession(PlayerCar player, ObstacleManager obstacles, RoadView road, System.Random rnd)
        {
            _player = player;
            _obstacles = obstacles;
            _road = road;
            _rnd = rnd ?? new System.Random();
        }

        public void Start()
        {
            Elapsed = 0f;
            Distance = 0f;
            Speed = Tuning.SpeedStart;
            SpeedT = 0f;
            Score = 0;
            Coins = 0;
            NearMisses = 0;
            ComboLevel = 1;
            ComboChain = 0;
            Revived = false;
            _crashed = false;
            _coinScore = 0;
            _nearMissScore = 0;
            _rowTimer = 0.85f;     // a short breather before the first row
            _comboTimer = 0f;
            _decayTimer = 0f;
            _speedMult = 1f;
            _reviveRamp = 0f;
            _slowTimer = 0f;
            _themeIndex = 0;
            _lastDifficulty = -1;
            _lastPattern = null;
            _queue.Clear();
            _tagged.Clear();

            _player.ResetForRun();
            _player.LaneChangeStarted -= HandleLaneChange;  // avoid double subscription
            _player.LaneChangeStarted += HandleLaneChange;
            _obstacles.ResetForRun();
            _road.ResetForRun();
        }

        public void Stop()
        {
            if (_player != null) _player.LaneChangeStarted -= HandleLaneChange;
        }

        public void Tick(float unscaledDt)
        {
            if (_crashed) return;

            float scale = 1f;
            if (_slowTimer > 0f)
            {
                _slowTimer -= unscaledDt;
                scale = 0.35f;
            }
            float dt = unscaledDt * scale;

            Elapsed += dt;

            float baseSpeed = Mathf.Min(Tuning.SpeedMax,
                Tuning.SpeedStart + (Elapsed / 10f) * Tuning.SpeedPerTenSeconds);

            if (_reviveRamp > 0f)
            {
                _reviveRamp -= dt;
                float k = 1f - Mathf.Clamp01(_reviveRamp / Tuning.ReviveRampSeconds);
                _speedMult = Mathf.Lerp(Tuning.ReviveSpeedFactor, 1f, k);
            }
            else
            {
                _speedMult = 1f;
            }

            Speed = baseSpeed * _speedMult;
            SpeedT = Mathf.Clamp01((Speed - Tuning.SpeedStart) / (Tuning.SpeedMax - Tuning.SpeedStart));
            Distance += Speed * dt;

            int theme = Mathf.FloorToInt(Distance / Tuning.MetresPerTheme) % Palette.Themes.Length;
            if (theme != _themeIndex)
            {
                _themeIndex = theme;
                _road.SetTheme(theme);
            }

            _player.Tick(dt);
            _obstacles.Tick(dt, Speed);
            _road.Tick(dt, Speed, SpeedT);

            _rowTimer -= dt;
            if (_rowTimer <= 0f)
            {
                SpawnNextRow();
                _rowTimer += RowInterval();
            }

            UpdateCombo(dt);
            CheckCoins();
            CheckNearMisses();

            Score = Mathf.FloorToInt(Distance / Tuning.MetresPerPoint) + _coinScore + _nearMissScore;

            CheckCollisions();
        }

        float RowInterval()
        {
            float t = Mathf.Clamp01(Elapsed / Tuning.RowIntervalRampSeconds);
            return Mathf.Lerp(Tuning.RowIntervalStart, Tuning.RowIntervalMin, t);
        }

        int MaxDifficulty
        {
            get
            {
                if (Elapsed < Tuning.WarmupSeconds) return 0;
                if (Elapsed < 60f) return 1;
                if (Elapsed < 120f) return 2;
                return 3;
            }
        }

        void FillQueue()
        {
            int max = MaxDifficulty;
            // breathing room after a hard pattern
            if (_lastDifficulty >= 2) max = Mathf.Min(max, 1);

            var pattern = PatternLibrary.Pick(max, _lastPattern, _rnd);
            var rows = PatternLibrary.Resolve(pattern, Elapsed, Distance, _rnd);

            if (!PatternLibrary.IsSolvable(rows, RowInterval(), _player.Lane))
            {
                // Fall back to something that is guaranteed to be reachable.
                rows = new List<RowSpec>();
                var safe = new RowSpec();
                int blocked = _rnd.Next(Layout.LaneCount);
                for (int i = 0; i < Layout.LaneCount; i++)
                {
                    safe.blocked[i] = i == blocked;
                    safe.kinds[i] = ObstacleKind.Car;
                }
                rows.Add(safe);
                pattern = null;
            }

            _queue.AddRange(rows);
            if (pattern != null)
            {
                _lastPattern = pattern;
                _lastDifficulty = pattern.difficulty;
            }
        }

        void SpawnNextRow()
        {
            if (_queue.Count == 0) FillQueue();
            if (_queue.Count == 0) return;

            var row = _queue[0];
            _queue.RemoveAt(0);

            for (int lane = 0; lane < Layout.LaneCount; lane++)
            {
                if (!row.blocked[lane]) continue;
                _obstacles.Spawn(row.kinds[lane], lane, Layout.SpawnY + _rnd.Next(0, 3) * 0.12f);
            }

            // coins in a free lane, slightly behind the row so they never sit inside traffic
            var free = row.FreeLanes();
            if (free.Count > 0 && _rnd.NextDouble() < 0.32)
            {
                int lane = free[_rnd.Next(free.Count)];
                bool line = _rnd.NextDouble() < 0.45;
                int count = line ? 3 : 1;
                for (int i = 0; i < count; i++)
                    _obstacles.SpawnCoin(lane, Layout.SpawnY + 2.2f + i * 3.4f);
            }
        }

        void HandleLaneChange(int fromLane)
        {
            if (_crashed) return;
            float laneX = Layout.LaneToX(fromLane);
            float py = _player.Y;

            var active = _obstacles.Active;
            for (int i = 0; i < active.Count; i++)
            {
                var o = active[i];
                if (o.awarded || o.nearMissTagged) continue;
                if (Mathf.Abs(o.x - laneX) > Tuning.NearMissLaneTolerance) continue;
                if (o.y < py - 0.5f) continue;                     // already behind us
                if (o.y > py + Tuning.NearMissWindow) continue;    // too far ahead to be a close call
                o.nearMissTagged = true;
                _tagged.Add(o);
            }
        }

        void CheckNearMisses()
        {
            float py = _player.Y;
            for (int i = _tagged.Count - 1; i >= 0; i--)
            {
                var o = _tagged[i];
                if (o == null || !o.active)
                {
                    _tagged.RemoveAt(i);
                    continue;
                }
                // fully passed the player without a collision
                if (o.y + o.halfH < py - Tuning.PlayerHitHalfH)
                {
                    o.awarded = true;
                    _tagged.RemoveAt(i);
                    AwardNearMiss();
                }
                else if (o.y < py - 12f)
                {
                    _tagged.RemoveAt(i);
                }
            }
        }

        void AwardNearMiss()
        {
            NearMisses++;
            int points = Tuning.NearMissBasePoints * ComboLevel;
            _nearMissScore += points;

            ComboChain++;
            if (ComboChain >= Tuning.ComboStep * ComboLevel && ComboLevel < Tuning.ComboMax)
            {
                ComboLevel++;
                ComboChain = 0;
                if (OnComboLevelUp != null) OnComboLevelUp();
            }
            else if (ComboLevel >= Tuning.ComboMax && ComboChain >= Tuning.ComboStep * Tuning.ComboMax)
            {
                ComboChain = 0;
            }

            _comboTimer = 0f;
            _decayTimer = 0f;
            _slowTimer = 0.09f;

            if (OnNearMiss != null) OnNearMiss(NearMisses, points);
        }

        void UpdateCombo(float dt)
        {
            if (NearMisses == 0 || ComboLevel <= 1) return;
            _comboTimer += dt;
            if (_comboTimer < Tuning.ComboGraceSeconds) return;

            _decayTimer += dt;
            if (_decayTimer >= Tuning.ComboDecayEvery)
            {
                _decayTimer = 0f;
                ComboLevel = Mathf.Max(1, ComboLevel - 1);
                ComboChain = 0;
            }
        }

        void CheckCoins()
        {
            var coins = _obstacles.ActiveCoins;
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                var c = coins[i];
                if (!c.active) continue;
                if (Mathf.Abs(c.x - _player.X) > 1.05f) continue;
                if (Mathf.Abs(c.y - _player.Y) > 1.45f) continue;

                _obstacles.CollectCoin(c);
                Coins++;
                _coinScore += Tuning.CoinPoints;
                if (OnCoin != null) OnCoin(Coins);
            }
        }

        void CheckCollisions()
        {
            if (_player.IsInvulnerable) return;

            float px, py, phw, phh;
            _player.GetBounds(out px, out py, out phw, out phh);

            var active = _obstacles.Active;
            for (int i = 0; i < active.Count; i++)
            {
                var o = active[i];
                if (Mathf.Abs(px - o.x) > phw + o.halfW * Tuning.ObstacleHitShrink) continue;
                if (Mathf.Abs(py - o.y) > phh + o.halfH * Tuning.ObstacleHitShrink) continue;

                Crash(o);
                return;
            }
        }

        void Crash(Obstacle cause)
        {
            _crashed = true;
            if (OnCrash != null) OnCrash();
        }

        /// <summary>Revive: kept combo, invulnerability, cleared road ahead, speed recovers.</summary>
        public void ApplyRevive()
        {
            Revived = true;
            _crashed = false;
            _speedMult = Tuning.ReviveSpeedFactor;
            _reviveRamp = Tuning.ReviveRampSeconds;
            _comboTimer = 0f;
            _decayTimer = 0f;
            _slowTimer = 0f;
            _player.SetInvulnerable(Tuning.ReviveInvulnSeconds);
            _obstacles.ClearAround(_player.Y - 3f, _player.Y + Tuning.ReviveClearRadius);
            _tagged.Clear();
            _queue.Clear();
            _rowTimer = Mathf.Max(_rowTimer, 1.1f);
        }

        public RunSummary Summary()
        {
            return new RunSummary
            {
                score = Score,
                metres = Metres,
                nearMisses = NearMisses,
                comboLevel = ComboLevel,
                coins = Coins,
                revived = Revived,
                seconds = Elapsed,
            };
        }
    }
}
