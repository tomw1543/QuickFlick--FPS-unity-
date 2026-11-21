using UnityEngine;

namespace QuickFlick.UI
{
    // Nebulae removed. Kept as a no-op so the file and its auto-run hook stay in place.
    public static class NebulaBuilder
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install() { }
    }
}
