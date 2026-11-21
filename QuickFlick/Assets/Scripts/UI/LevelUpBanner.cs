using QuickFlick.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    // Centre-screen "LEVEL UP - <COLOR>" flash. Uses legacy Text like the rest of the HUD (no TMP essentials imported).
    public class LevelUpBanner : MonoBehaviour
    {
        const float FadeIn = 0.1f, Hold = 0.7f, FadeOut = 0.3f;
        static readonly string[] Names = { "CYAN", "BLUE", "RED", "GREEN", "PURPLE", "WHITE", "RAINBOW" };

        Text label;
        Color tint;
        float t = float.MaxValue;

        void Awake()
        {
            label = HudRoot.NewText(transform, "LevelUpLabel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400, 120), TextAnchor.MiddleCenter, 72);
            label.color = Color.clear;
        }

        void OnEnable() => GameEvents.LevelUp += OnLevelUp;
        void OnDisable() => GameEvents.LevelUp -= OnLevelUp;

        void OnLevelUp(int level, Color laserColor)
        {
            label.text = "LEVEL UP — " + Names[Mathf.Clamp(level, 0, Names.Length - 1)];
            tint = laserColor;
            t = 0f;
        }

        void Update()
        {
            if (t >= FadeIn + Hold + FadeOut) return;
            t += Time.unscaledDeltaTime;
            float a = t < FadeIn ? t / FadeIn : t < FadeIn + Hold ? 1f : 1f - (t - FadeIn - Hold) / FadeOut;
            label.color = new Color(tint.r, tint.g, tint.b, Mathf.Clamp01(a));
        }
    }
}
