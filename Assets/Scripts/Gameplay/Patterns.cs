using System.Collections.Generic;
using UnityEngine;

namespace EdgeRush
{
    /// <summary>One lateral slice of traffic. At most two lanes are ever blocked.</summary>
    public class RowSpec
    {
        public bool[] blocked = new bool[Layout.LaneCount];
        public ObstacleKind[] kinds = new ObstacleKind[Layout.LaneCount];

        public int BlockedCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < blocked.Length; i++) if (blocked[i]) n++;
                return n;
            }
        }

        public bool IsFree(int lane) => lane >= 0 && lane < Layout.LaneCount && !blocked[lane];

        public List<int> FreeLanes()
        {
            var list = new List<int>(Layout.LaneCount);
            for (int i = 0; i < blocked.Length; i++) if (!blocked[i]) list.Add(i);
            return list;
        }
    }

    public class Pattern
    {
        public string id;
        public int difficulty;   // 0 warmup .. 3 extreme
        public string[] rows;    // '.' free, 'X' random obstacle, 'T' truck, 'B' barrier, 'M' mover
    }

    /// <summary>
    /// Hand designed traffic patterns plus the fairness checks from section 4.4 of the design
    /// document: every row leaves a lane open and every pattern is reachable from the lane the
    /// player is in.
    /// </summary>
    public static class PatternLibrary
    {
        public static readonly Pattern[] All =
        {
            // ---- difficulty 0: single obstacle rows (warmup) -------------------
            new Pattern { id = "s_left",   difficulty = 0, rows = new[] { "X.." } },
            new Pattern { id = "s_mid",    difficulty = 0, rows = new[] { ".X." } },
            new Pattern { id = "s_right",  difficulty = 0, rows = new[] { "..X" } },
            new Pattern { id = "s_weave",  difficulty = 0, rows = new[] { "X..", "..X" } },
            new Pattern { id = "s_wl",     difficulty = 0, rows = new[] { ".X.", "X.." } },
            new Pattern { id = "s_wr",     difficulty = 0, rows = new[] { ".X.", "..X" } },

            // ---- difficulty 1: two obstacles, one lane stays open -------------
            new Pattern { id = "d_left",   difficulty = 1, rows = new[] { "XX." } },
            new Pattern { id = "d_right",  difficulty = 1, rows = new[] { ".XX" } },
            new Pattern { id = "d_gate",   difficulty = 1, rows = new[] { "X.X" } },
            new Pattern { id = "d_step",   difficulty = 1, rows = new[] { "XX.", ".XX" } },
            new Pattern { id = "d_step2",  difficulty = 1, rows = new[] { ".XX", "XX." } },
            new Pattern { id = "d_double", difficulty = 1, rows = new[] { "X.X", "X.X" } },
            new Pattern { id = "d_box",    difficulty = 1, rows = new[] { "XX.", "XX." } },

            // ---- difficulty 2: sequences that force movement ------------------
            new Pattern { id = "z_zig",    difficulty = 2, rows = new[] { "XX.", "X.X", ".XX" } },
            new Pattern { id = "z_zag",    difficulty = 2, rows = new[] { ".XX", "X.X", "XX." } },
            new Pattern { id = "z_slalom", difficulty = 2, rows = new[] { "X..", ".X.", "..X", ".X." } },
            new Pattern { id = "z_pinch",  difficulty = 2, rows = new[] { "X.X", ".X.", "X.X" } },
            new Pattern { id = "z_rails",  difficulty = 2, rows = new[] { "XX.", ".XX", "XX." } },
            new Pattern { id = "z_stair",  difficulty = 2, rows = new[] { "X..", "XX.", ".XX", "..X" } },
            new Pattern { id = "z_truck",  difficulty = 2, rows = new[] { "T..", ".X.", "..T" } },
            new Pattern { id = "z_barrier",difficulty = 2, rows = new[] { "X.X", ".B.", "X.X" } },

            // ---- difficulty 3: long, high pressure ----------------------------
            new Pattern { id = "x_gauntlet", difficulty = 3, rows = new[] { "XX.", "X.X", ".XX", "X.X" } },
            new Pattern { id = "x_squeeze",  difficulty = 3, rows = new[] { ".XX", ".XX", "XX." } },
            new Pattern { id = "x_mover",    difficulty = 3, rows = new[] { "X..", ".M.", "..X", ".X." } },
            new Pattern { id = "x_long",     difficulty = 3, rows = new[] { "XX.", ".XX", "X.X", ".XX", "XX." } },
            new Pattern { id = "x_mix",      difficulty = 3, rows = new[] { "T.X", ".X.", "X.B", "B.X" } },
        };

        public static Pattern Pick(int maxDifficulty, Pattern last, System.Random rnd)
        {
            maxDifficulty = Mathf.Clamp(maxDifficulty, 0, 3);
            var candidates = new List<Pattern>();
            for (int i = 0; i < All.Length; i++)
            {
                var p = All[i];
                if (p.difficulty > maxDifficulty) continue;
                if (last != null && p.id == last.id) continue;
                candidates.Add(p);
            }
            if (candidates.Count == 0)
            {
                for (int i = 0; i < All.Length; i++)
                    if (All[i].difficulty == 0) candidates.Add(All[i]);
            }
            return candidates[rnd.Next(candidates.Count)];
        }

        /// <summary>Resolves a pattern into concrete rows for the current point in the run.</summary>
        public static List<RowSpec> Resolve(Pattern pattern, float elapsed, float metres, System.Random rnd)
        {
            var rows = new List<RowSpec>(pattern.rows.Length);
            foreach (var text in pattern.rows)
            {
                var row = new RowSpec();
                for (int lane = 0; lane < Layout.LaneCount && lane < text.Length; lane++)
                {
                    char c = text[lane];
                    if (c == '.') continue;
                    row.blocked[lane] = true;
                    row.kinds[lane] = ResolveKind(c, elapsed, metres, rnd);
                }
                // never allow a fully blocked row
                if (row.BlockedCount >= Layout.LaneCount)
                {
                    int open = rnd.Next(Layout.LaneCount);
                    row.blocked[open] = false;
                }
                rows.Add(row);
            }
            return rows;
        }

        static ObstacleKind ResolveKind(char c, float elapsed, float metres, System.Random rnd)
        {
            switch (c)
            {
                case 'T':
                    return elapsed >= 15f ? ObstacleKind.Truck : ObstacleKind.Car;
                case 'B':
                    return ObstacleKind.Barrier;
                case 'M':
                    return metres >= 800f ? ObstacleKind.Mover : ObstacleKind.Car;
                default:
                    // 'X' picks a warm obstacle appropriate to the current stage
                    float r = (float)rnd.NextDouble();
                    if (elapsed < 15f) return r < 0.55f ? ObstacleKind.Car : ObstacleKind.Barrier;
                    if (metres >= 800f)
                    {
                        if (r < 0.62f) return ObstacleKind.Car;
                        if (r < 0.78f) return ObstacleKind.Truck;
                        if (r < 0.92f) return ObstacleKind.Barrier;
                        return ObstacleKind.Mover;
                    }
                    if (r < 0.68f) return ObstacleKind.Car;
                    if (r < 0.84f) return ObstacleKind.Truck;
                    return ObstacleKind.Barrier;
            }
        }

        /// <summary>
        /// Simulates the set of lanes the player can be in when each row arrives. If a row can
        /// not be reached from any lane, the pattern would be unfair and is rejected.
        /// </summary>
        public static bool IsSolvable(List<RowSpec> rows, float rowInterval, int startLane)
        {
            var reachable = new HashSet<int> { startLane };
            int movesPerRow = Mathf.Max(1, Mathf.FloorToInt(rowInterval / Mathf.Max(0.01f, Tuning.LaneChangeTime)));

            foreach (var row in rows)
            {
                var next = new HashSet<int>();
                var free = row.FreeLanes();
                if (free.Count == 0) return false;
                foreach (var lane in reachable)
                {
                    foreach (var f in free)
                        if (Mathf.Abs(f - lane) <= movesPerRow) next.Add(f);
                }
                if (next.Count == 0) return false;
                reachable = next;
            }
            return true;
        }
    }
}
