using UnityEngine;

namespace EdgeRush
{
    /// <summary>Build-time strings. Change these before you publish.</summary>
    public static class BuildInfo
    {
        public const string Version = "0.1.0";
        public const string GameName = "Edge Rush";

        // TODO: replace with the real URL you host the privacy policy on.
        public const string PrivacyPolicyUrl = "https://example.com/edge-rush/privacy";

        // Proposed publisher identity. Verify availability before the first store upload.
        // Must match Player Settings > Android > Package Name and the developer center.
        public const string PackageName = "com.gizmobyte.edgerush";
    }
}
