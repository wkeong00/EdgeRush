using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EdgeRush.EditorTools
{
    /// <summary>Stop a release before it can silently ship Mock or Google's sample publisher IDs.</summary>
    public sealed class AdBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;

        public static void RunChecks() => AdRegressionChecks.Run();

        public void OnPreprocessBuild(BuildReport report)
        {
            if ((report.summary.options & BuildOptions.Development) != 0 || !AdConfiguration.AdsEnabled) return;
            if (report.summary.platform != BuildTarget.Android && report.summary.platform != BuildTarget.iOS)
                throw new BuildFailedException("EdgeRush release ads have no provider for this target. Disable AdsEnabled explicitly for an ad-free build.");
            var group = BuildPipeline.GetBuildTargetGroup(report.summary.platform);
            var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
            bool hasProvider = Array.IndexOf(defines.Split(';'), "EDGERUSH_ADMOB") >= 0;
            if (!hasProvider)
                throw new BuildFailedException("EdgeRush release blocked: install/configure the selected real ad SDK and enable EDGERUSH_ADMOB, or explicitly disable AdsEnabled. Mock is Development/Editor only.");
            if (!AdConfiguration.IsConfigured())
                throw new BuildFailedException("EdgeRush release blocked: every enabled rewarded placement needs a valid production unit ID. Missing IDs and Google's test publisher 3940256099942544 are rejected.");
        }
    }

    public static class AdRegressionChecks
    {
        [MenuItem("EdgeRush/Checks/Ad Configuration and Callback Guards")]
        public static void Run()
        {
            Require(!AdConfiguration.IsProductionUnitId(null), "Null unit ID rejected");
            Require(!AdConfiguration.IsProductionUnitId(" "), "Blank unit ID rejected");
            Require(!AdConfiguration.IsProductionUnitId("ca-app-pub-3940256099942544/5224354917"), "Google test publisher rejected");
            Require(!AdConfiguration.IsProductionUnitId("ca-app-pub-1234567890123456/not-a-unit"), "Malformed unit ID rejected");
            Require(AdConfiguration.IsProductionUnitId("ca-app-pub-1234567890123456/1234567890"), "Production ID format accepted");

            var completed = new AdCompletionGuard();
            Require(completed.TryReward() && !completed.TryReward(), "Duplicate reward rejected");
            Require(completed.TryClose() && !completed.TryClose(), "Duplicate close/failure rejected");
            Require(completed.Rewarded, "Close/failure preserves earned eligibility");
            var skipped = new AdCompletionGuard();
            Require(skipped.TryClose() && !skipped.Rewarded && !skipped.TryReward(), "Skip closes without reward; stale reward rejected");
            Debug.Log("[Checks] Ad configuration and callback guards passed (9 assertions). Live Mock completion/skip/close/failure and SDK flows require Play Mode/device QA.");
        }

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Ad regression: " + message);
        }
    }
}
