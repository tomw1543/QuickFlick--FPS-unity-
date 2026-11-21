using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace QuickFlick.UI
{
    /// <summary>Builds a global URP volume (Bloom, Vignette, ACES tonemapping) at runtime.</summary>
    public static class PostFxBuilder
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Install()
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("PostFxBuilder: no Main Camera found, skipping post-processing.");
                return;
            }

            var camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null) camData.renderPostProcessing = true;
            else Debug.LogWarning("PostFxBuilder: Main Camera has no UniversalAdditionalCameraData (is URP active?).");

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            // Bloom threshold was 0.9 with ACES tonemapping — everything the sun lit bleached to white.
            // Threshold 1.5 means only true HDR highlights (emissive effects, projectile glow) bloom.
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.6f);
            bloom.threshold.Override(1.5f);
            bloom.scatter.Override(0.5f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.35f);
            vignette.smoothness.Override(0.5f);

            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            var go = new GameObject("PostFx");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;
            volume.sharedProfile = profile;
            Object.DontDestroyOnLoad(go);
        }
    }
}
