using QuickFlick.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    public class WaveBanner : MonoBehaviour
    {
        const float Duration = 2f, Fade = 0.5f;

        Text text;
        float timer; // seconds left; 0 = hidden

        void Awake()
        {
            text = HudRoot.NewText(transform, "Banner", new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(800, 100), TextAnchor.MiddleCenter, 64);
            SetAlpha(0);
        }

        void OnEnable() { GameEvents.WaveStart += OnStart; GameEvents.WaveClear += OnClear; }
        void OnDisable() { GameEvents.WaveStart -= OnStart; GameEvents.WaveClear -= OnClear; }

        void OnStart(int wave) => Show($"WAVE {wave}");
        void OnClear(int wave) => Show("CLEAR");

        void Show(string s) { text.text = s; timer = Duration; }

        void Update()
        {
            if (timer <= 0f) return;
            timer = Mathf.Max(0f, timer - Time.unscaledDeltaTime);
            SetAlpha(Mathf.Clamp01(Mathf.Min(Duration - timer, timer) / Fade));
        }

        void SetAlpha(float a) { var c = text.color; c.a = a; text.color = c; }
    }
}
