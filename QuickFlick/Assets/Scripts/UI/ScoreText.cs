using QuickFlick.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    public class ScoreText : MonoBehaviour
    {
        Text text;

        void Awake()
        {
            text = HudRoot.NewText(transform, "Score", Vector2.one, new Vector2(-20, -16), new Vector2(400, 40), TextAnchor.UpperRight, 28);
            OnScore(0);
        }

        void OnEnable() => GameEvents.ScoreChanged += OnScore;
        void OnDisable() => GameEvents.ScoreChanged -= OnScore;

        void OnScore(int score) => text.text = $"SCORE  {score}";
    }
}
