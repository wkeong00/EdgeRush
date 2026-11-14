using System;
using UnityEditor;
using UnityEngine;

namespace EdgeRush.EditorTools
{
    /// <summary>Deterministic input/clock checks. Run from the menu or Unity -executeMethod.</summary>
    public static class GameplayChecks
    {
        static int _assertions;

        [MenuItem("EdgeRush/Checks/Gameplay Input and Clocks")]
        public static void Run()
        {
            _assertions = 0;
            CheckGestures();
            CheckPlayerClock();
            Debug.Log("[Checks] Gameplay input and clocks passed (" + _assertions + " assertions). Touch feel and tutorial readability still require Play Mode/device QA.");
        }

        static void CheckGestures()
        {
            var input = new GameplayGesture();
            Require(input.Move(1, 200, 100, 40) == 0, "Movement without a press ignored");
            input.Begin(1, 500, 100, 0);
            Require(input.Move(1, 550, 100, 40) == 1, "Horizontal swipe fires immediately");
            Require(input.Move(1, 400, 100, 40) == 0, "A reversal cannot fire a second move");
            Require(input.End(1, 550, 100, 0.15f, 1080, 40) == 0, "Swipe release cannot become a tap");
            Require(input.End(1, 550, 100, 0.16f, 1080, 40) == 0, "Duplicate release ignored");

            input.Begin(1, 100, 100, 1);
            Require(input.End(1, 101, 101, 1.1f, 1080, 40) == -1, "Left half tap");
            input.Begin(1, 900, 100, 2);
            Require(input.End(1, 901, 101, 2.1f, 1080, 40) == 1, "Right half tap");
            input.Begin(1, 500, 100, 3);
            Require(input.End(1, 440, 100, 3.1f, 1080, 40) == -1, "Release-only swipe still emits once");
            input.Begin(1, 500, 100, 4);
            Require(input.Move(1, 510, 180, 40) == 0 && input.End(1, 510, 180, 4.1f, 1080, 40) == 0,
                "Vertical drag cannot become a lane change");
            input.Begin(1, 500, 100, 5);
            Require(input.End(1, 500, 100, 6, 1080, 40) == 0, "Long hold cannot become a tap");

            input.Begin(1, 500, 100, 7);
            Require(input.Move(2, 600, 100, 40) == 0, "Other pointer cannot move the car");
            Require(input.End(2, 600, 100, 7.1f, 1080, 40) == 0, "Other pointer cannot release the owner");
            Require(input.Move(1, 440, 100, 40) == -1, "Owner survives unrelated pointer callbacks");
            input.Cancel();
            input.Begin(1, 500, 100, 8);
            input.Begin(2, 800, 100, 8.05f);
            Require(input.Move(1, 440, 100, 40) == -1, "A second press cannot take ownership");
            input.Cancel();
            Require(input.Move(1, 800, 100, 40) == 0 && input.End(1, 800, 100, 8.1f, 1080, 40) == 0,
                "Cancel invalidates both move and release");
        }

        static void CheckPlayerClock()
        {
            var go = new GameObject("GameplayChecks.Player") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var player = go.AddComponent<PlayerCar>();
                player.ResetForRun();
                player.SetInvulnerable(1f);
                Require(player.IsInvulnerable, "Protection starts immediately");
                player.Tick(0f);
                Require(player.IsInvulnerable, "Frozen gameplay clock preserves protection");
                player.Tick(0.2f);
                player.SetInvulnerable(0.1f);
                player.Tick(0.79f);
                Require(player.IsInvulnerable, "A shorter protection request cannot shorten an existing source");
                player.Tick(0.02f);
                Require(!player.IsInvulnerable, "Protection expires on effective gameplay time");
                player.SetInvulnerable(2f);
                player.ResetForRun();
                Require(!player.IsInvulnerable, "Reset clears protection");

                int arrivals = 0;
                player.LaneChangeFinished += lane => arrivals++;
                player.Move(-1);
                player.Tick(Tuning.LaneChangeTime * 0.5f);
                float halfway = player.X;
                Require(halfway < Layout.LaneToX(1) && halfway > Layout.LaneToX(0), "Lane change animates towards the target");
                player.Tick(0f);
                Require(Mathf.Abs(player.X - halfway) < 0.0001f, "Frozen gameplay clock freezes a lane change");
                player.Tick(Tuning.LaneChangeTime * 0.51f);
                Require(Mathf.Abs(player.X - Layout.LaneToX(0)) < 0.0001f && arrivals == 1,
                    "Lane change finishes in the baseline effective time exactly once");

                player.ResetForRun();
                player.Move(-1);
                player.Move(1);
                player.CancelBufferedMove();
                player.Tick(Tuning.LaneChangeTime * 1.01f);
                player.Tick(Tuning.LaneChangeTime * 1.01f);
                Require(player.Lane == 0 && Mathf.Abs(player.X - Layout.LaneToX(0)) < 0.0001f,
                    "Pause/input cancellation removes the queued next lane change");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        static void Require(bool condition, string message)
        {
            _assertions++;
            if (!condition) throw new InvalidOperationException("Gameplay regression: " + message);
        }
    }
}
