using UnityEngine;

namespace EdgeRush
{
    /// <summary>
    /// Creates the game from an empty scene. The project ships without a authored scene, so the
    /// whole world is built in code at runtime - nothing to break when scenes are re-imported.
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindAnyObjectByType<GameApp>() != null) return;
            var go = new GameObject("EdgeRush");
            go.AddComponent<GameApp>();
        }
    }
}

