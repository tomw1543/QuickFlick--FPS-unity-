using UnityEngine;

namespace QuickFlick.Systems
{
    public class PointLevel : MonoBehaviour
    {
        const int PointsPerLevel = 2000;
        const int RainbowLevel = 6;

        // Keep hex values in sync with LevelUpBanner.
        public static readonly Color[] Palette =
        {
            Hex(0x00, 0xE5, 0xFF), // cyan
            Hex(0x2E, 0x6B, 0xFF), // blue
            Hex(0xFF, 0x2E, 0x2E), // red
            Hex(0x2E, 0xFF, 0x6B), // green
            Hex(0xA3, 0x2E, 0xFF), // purple
            Hex(0xFF, 0xFF, 0xFF), // white
        };

        public static int CurrentLevel { get; private set; }
        // Stable color for UI; white for rainbow.
        public static Color CurrentLaserColor => Palette[Mathf.Min(CurrentLevel, Palette.Length - 1)];

        static int rainbowIndex;

        static Color Hex(byte r, byte g, byte b) => new Color32(r, g, b, 255);

        /// <summary>Color for the next shot; advances the rainbow cycle at level 6+.</summary>
        public static Color NextShotColor()
        {
            if (CurrentLevel < RainbowLevel) return CurrentLaserColor;
            return Palette[1 + rainbowIndex++ % 5]; // the five solids after cyan: levels 1..5
        }

        void OnEnable()
        {
            CurrentLevel = 0; rainbowIndex = 0;
            GameEvents.ScoreChanged += OnScore;
            GameEvents.Restart += OnRestart;
        }

        void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScore;
            GameEvents.Restart -= OnRestart;
        }

        void OnRestart() { CurrentLevel = 0; rainbowIndex = 0; }

        void OnScore(int score)
        {
            int level = score / PointsPerLevel;
            bool up = level > CurrentLevel;
            CurrentLevel = level;
            if (up) GameEvents.RaiseLevelUp(level, CurrentLaserColor);
        }
    }
}
