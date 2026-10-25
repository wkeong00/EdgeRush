using UnityEngine;

namespace EdgeRush
{
    /// <summary>
    /// Fixed world layout. The camera is orthographic and shows a constant number of world
    /// units vertically (10 half-height), so the play field looks identical on every phone.
    /// Taller screens simply reveal a little more road ahead.
    /// </summary>
    public static class Layout
    {
        public const int LaneCount = 3;
        public const float LaneWidth = 2.7f;
        public const float RoadHalf = LaneWidth * LaneCount * 0.5f;   // 4.05

        public const float CarW = 1.90f;
        public const float CarH = 3.50f;
        public const float TruckW = 2.16f;
        public const float TruckH = 5.5f;
        public const float BarrierW = 1.95f;
        public const float BarrierH = 1.0f;

        // Collision hitboxes are ~10% smaller than the visuals (feels fairer).
        public const float HitShrink = 0.9f;

        public static readonly float[] LaneX = { -LaneWidth, 0f, LaneWidth };

        public static float OrthoSize { get; private set; } = 10f;
        public static float PlayerY { get; private set; } = -6.2f;
        public static float SpawnY { get; private set; } = 14f;
        public static float DespawnY { get; private set; } = -14f;
        public static float ViewHalfWidth { get; private set; } = 5.62f;

        public static void Init(float aspect)
        {
            aspect = Mathf.Max(0.3f, aspect);
            OrthoSize = Mathf.Max(10f, (RoadHalf + 0.55f) / aspect);
            ViewHalfWidth = OrthoSize * aspect;
            PlayerY = -OrthoSize * 0.60f;
            SpawnY = OrthoSize + 4.0f;
            DespawnY = -OrthoSize - 4.5f;
        }

        public static float LaneToX(int lane) => LaneX[Mathf.Clamp(lane, 0, LaneCount - 1)];

        public static int XToLane(float x)
        {
            if (x < -LaneWidth * 0.5f) return 0;
            if (x > LaneWidth * 0.5f) return 2;
            return 1;
        }

        /// <summary>Distance (world units) driven per metre used for the "xx m" readout.</summary>
        public const float MetresPerUnit = 1f;
    }

    public static class Tuning
    {
        // ---- speed -------------------------------------------------------------
        public const float SpeedStart = 8f;
        public const float SpeedPerTenSeconds = 1f;
        public const float SpeedMax = 22f;

        // ---- spawning ----------------------------------------------------------
        public const float RowIntervalStart = 1.6f;
        public const float RowIntervalMin = 0.55f;
        public const float RowIntervalRampSeconds = 120f;
        public const float WarmupSeconds = 15f;
        public const float LaneChangeTime = 0.12f;
        public const float PlayerHitHalfW = Layout.CarW * Layout.HitShrink * 0.5f;
        public const float PlayerHitHalfH = Layout.CarH * Layout.HitShrink * 0.5f;
        public const float ObstacleHitShrink = 0.88f;

        // ---- scoring -----------------------------------------------------------
        public const float MetresPerPoint = 10f;
        public const int CoinPoints = 5;
        public const int NearMissBasePoints = 10;
        public const float NearMissWindow = Layout.CarH * 1.5f;   // 1.5 car lengths ahead
        public const float NearMissLaneTolerance = Layout.LaneWidth * 0.72f;
        public const int ComboStep = 3;                    // 3 near misses => +1 multiplier
        public const int ComboMax = 5;
        public const float ComboGraceSeconds = 5f;
        public const float ComboDecayEvery = 1.2f;

        // ---- revive ------------------------------------------------------------
        public const float ReviveInvulnSeconds = 2f;
        public const float ReviveSpeedFactor = 0.8f;
        public const float ReviveRampSeconds = 3f;
        public const float ReviveClearRadius = 26f;
        public const int ReviveAdOfferAfterRuns = 3;

        // ---- themes ------------------------------------------------------------
        public const float MetresPerTheme = 500f;

        // ---- balancing of ad economy ------------------------------------------
        public const int DailyAdMax = 3;
        public const int DailyAdCoins = 30;
        public const int SkinAdCount = 5;
        public const int SkinAdRewardCoins = 40;
    }
}

