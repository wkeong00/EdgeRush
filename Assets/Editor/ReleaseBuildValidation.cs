using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EdgeRush.EditorTools
{
    /// <summary>Non-secret configuration only. No password, key contents or live PlayerSettings mutations.</summary>
    public sealed class ReleaseBuildSnapshot
    {
        public string packageName;
        public string expectedPackageName;
        public string companyName;
        public string version;
        public string expectedVersion;
        public int versionCode;
        public string privacyPolicyUrl;
        public bool useCustomKeystore;
        public string keystorePath;
        public string keystoreAlias;
    }

    public sealed class ReleaseBuildIssue
    {
        public readonly string code;
        public readonly string message;
        public ReleaseBuildIssue(string code, string message) { this.code = code; this.message = message; }
        public override string ToString() => code + ": " + message;
    }

    /// <summary>
    /// Android Release identity checks run before the ad configuration guard, even when ads are disabled.
    /// This validates source configuration; the final APK/AAB and certificate still require inspection.
    /// </summary>
    public sealed class ReleaseBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!AppliesTo(report.summary.platform == BuildTarget.Android,
                (report.summary.options & BuildOptions.Development) != 0)) return;
            var issues = Validate(CaptureCurrent());
            if (issues.Count == 0) return;
            throw new BuildFailedException("EdgeRush Android Release configuration is incomplete:\n- " +
                string.Join("\n- ", issues.ConvertAll(issue => issue.ToString())) +
                "\nThis checks configuration only. Verify the final artifact and signing certificate separately.");
        }

        public static bool AppliesTo(bool android, bool development) => android && !development;

        public static ReleaseBuildSnapshot CaptureCurrent()
        {
            return new ReleaseBuildSnapshot
            {
                packageName = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android),
                expectedPackageName = BuildInfo.PackageName,
                companyName = PlayerSettings.companyName,
                version = PlayerSettings.bundleVersion,
                expectedVersion = BuildInfo.Version,
                versionCode = PlayerSettings.Android.bundleVersionCode,
                privacyPolicyUrl = BuildInfo.PrivacyPolicyUrl,
                useCustomKeystore = PlayerSettings.Android.useCustomKeystore,
                keystorePath = PlayerSettings.Android.keystoreName,
                keystoreAlias = PlayerSettings.Android.keyaliasName,
            };
        }

        /// <summary>Pure input validation. Does not access Unity settings, the network or keystore files.</summary>
        public static List<ReleaseBuildIssue> Validate(ReleaseBuildSnapshot input)
        {
            var issues = new List<ReleaseBuildIssue>();
            if (input == null)
            {
                Add(issues, "snapshot_missing", "A configuration snapshot is required.");
                return issues;
            }
            CheckPackage(issues, input.packageName, "player_package", "PlayerSettings Android package");
            CheckPackage(issues, input.expectedPackageName, "buildinfo_package", "BuildInfo.PackageName");
            if (!string.Equals(input.packageName, input.expectedPackageName, StringComparison.Ordinal))
                Add(issues, "package_mismatch", "PlayerSettings Android package must match BuildInfo.PackageName exactly.");
            if (IsPlaceholderCompany(input.companyName))
                Add(issues, "company_placeholder", "Set a real non-empty company/developer name in PlayerSettings.");
            if (string.IsNullOrWhiteSpace(input.version) || string.IsNullOrWhiteSpace(input.expectedVersion))
                Add(issues, "version_missing", "PlayerSettings.bundleVersion and BuildInfo.Version must both be non-empty.");
            if (!string.Equals(input.version, input.expectedVersion, StringComparison.Ordinal))
                Add(issues, "version_mismatch", "PlayerSettings.bundleVersion must match BuildInfo.Version exactly.");
            if (input.versionCode <= 0)
                Add(issues, "version_code_invalid", "Android versionCode must be positive; track increases between published versions separately.");
            if (!IsPolicyAddressConfigured(input.privacyPolicyUrl))
                Add(issues, "privacy_url_invalid", "Set an absolute HTTPS privacy policy URL on a non-placeholder host; check public availability separately.");
            if (!input.useCustomKeystore)
                Add(issues, "custom_signing_disabled", "Enable Android custom keystore signing for Release.");
            if (string.IsNullOrWhiteSpace(input.keystorePath))
                Add(issues, "keystore_path_missing", "Select a non-empty Android keystore path; no password is inspected or logged.");
            if (string.IsNullOrWhiteSpace(input.keystoreAlias))
                Add(issues, "keystore_alias_missing", "Select a non-empty Android key alias; verify the resulting certificate after building.");
            return issues;
        }

        static void CheckPackage(List<ReleaseBuildIssue> issues, string value, string code, string label)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                !Regex.IsMatch(value, @"\A[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+\z", RegexOptions.CultureInvariant))
            {
                Add(issues, code + "_invalid", label + " must be a non-empty Android package identifier.");
                return;
            }
            foreach (var segment in value.Split('.'))
            {
                switch (segment.ToLowerInvariant())
                {
                    case "example": case "yourstudio": case "yourcompany": case "defaultcompany":
                    case "companyname": case "placeholder": case "productname":
                        Add(issues, code + "_placeholder", label + " still contains a sample/placeholder segment.");
                        return;
                }
            }
        }

        static bool IsPlaceholderCompany(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            string normalized = Regex.Replace(value, @"[\s_-]", "").ToLowerInvariant();
            switch (normalized)
            {
                case "yourstudio": case "yourcompany": case "defaultcompany": case "companyname":
                case "example": case "examplestudio": case "examplecompany": case "placeholder":
                    return true;
                default: return false;
            }
        }

        static bool IsPolicyAddressConfigured(string value)
        {
            Uri address;
            if (!Uri.TryCreate(value, UriKind.Absolute, out address) || address.Scheme != Uri.UriSchemeHttps ||
                address.HostNameType != UriHostNameType.Dns || !string.IsNullOrEmpty(address.UserInfo)) return false;
            string host = address.DnsSafeHost.TrimEnd('.');
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || !host.Contains(".")) return false;
            foreach (var suffix in new[] { ".invalid", ".test", ".localhost", ".example", ".local" })
                if (host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return false;
            foreach (var placeholder in new[] { "example.com", "example.net", "example.org" })
                if (host.Equals(placeholder, StringComparison.OrdinalIgnoreCase) ||
                    host.EndsWith("." + placeholder, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        static void Add(List<ReleaseBuildIssue> issues, string code, string message) =>
            issues.Add(new ReleaseBuildIssue(code, message));
    }

    /// <summary>Checks use only synthetic snapshots, leaving actual PlayerSettings and signing data untouched.</summary>
    public static class ReleaseBuildRegressionChecks
    {
        [MenuItem("EdgeRush/Checks/Release Configuration Guards")]
        public static void Run()
        {
            Require(ReleaseBuildValidation.Validate(Valid()).Count == 0, "Valid synthetic configuration rejected");
            Require(ReleaseBuildValidation.AppliesTo(true, false) && !ReleaseBuildValidation.AppliesTo(true, true) &&
                !ReleaseBuildValidation.AppliesTo(false, false), "Only Android non-Development builds should be guarded");

            var snapshot = Valid();
            snapshot.packageName = snapshot.expectedPackageName = "com.yourstudio.edgerush";
            snapshot.companyName = "Your Studio";
            Require(Has(snapshot, "player_package_placeholder") && Has(snapshot, "buildinfo_package_placeholder") &&
                Has(snapshot, "company_placeholder"), "Project example identity must not pass Release validation");

            snapshot = Valid(); snapshot.packageName = ""; snapshot.companyName = " ";
            Require(Has(snapshot, "player_package_invalid") && Has(snapshot, "company_placeholder"), "Blank identity must be rejected");
            snapshot = Valid(); snapshot.packageName = "com.other.edgerush"; snapshot.version = "2.0.0";
            Require(Has(snapshot, "package_mismatch") && Has(snapshot, "version_mismatch"), "BuildInfo/PlayerSettings disagreement must be rejected");
            snapshot = Valid(); snapshot.expectedVersion = ""; snapshot.versionCode = 0;
            Require(Has(snapshot, "version_missing") && Has(snapshot, "version_code_invalid"), "Missing version and nonpositive versionCode must be rejected");

            foreach (var url in new[] { "", "http://privacy.regression-studio.com/policy", "https://example.com/privacy", "https://sub.example.com/privacy", "https://studio.invalid/privacy" })
            {
                snapshot = Valid(); snapshot.privacyPolicyUrl = url;
                Require(Has(snapshot, "privacy_url_invalid"), "Insecure or placeholder privacy URL must be rejected");
            }
            snapshot = Valid(); snapshot.useCustomKeystore = false;
            Require(Has(snapshot, "custom_signing_disabled"), "Default signing must not pass Release validation");
            snapshot = Valid(); snapshot.keystorePath = " "; snapshot.keystoreAlias = "";
            Require(Has(snapshot, "keystore_path_missing") && Has(snapshot, "keystore_alias_missing"), "Both signing fields are required");

            Debug.Log("[Checks] Release configuration guards passed (13 assertions). Synthetic snapshots only; no PlayerSettings changed or signing credentials read. Actual APK/AAB signatures and policy availability remain separate checks.");
        }

        static ReleaseBuildSnapshot Valid() => new ReleaseBuildSnapshot
        {
            packageName = "com.regression.edgerush", expectedPackageName = "com.regression.edgerush",
            companyName = "Regression Studio", version = "1.0.0", expectedVersion = "1.0.0", versionCode = 1,
            privacyPolicyUrl = "https://privacy.regression-studio.com/policy", useCustomKeystore = true,
            keystorePath = "synthetic-release.keystore", keystoreAlias = "synthetic-release",
        };

        static bool Has(ReleaseBuildSnapshot snapshot, string code) =>
            ReleaseBuildValidation.Validate(snapshot).Exists(issue => issue.code == code);
        static void Require(bool passed, string message)
        {
            if (!passed) throw new InvalidOperationException("Release configuration regression: " + message);
        }
    }
}
