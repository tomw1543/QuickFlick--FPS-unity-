using QuickFlick.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    public class HpPips : MonoBehaviour
    {
        const int Count = 5;
        const float Size = 16f, Gap = 4f, Margin = 20f;
        static readonly Color Lit = new Color(0f, 0.898f, 1f);      // #00E5FF
        static readonly Color Unlit = new Color(0.2f, 0.2f, 0.2f);  // #333333

        readonly Image[] pips = new Image[Count];

        void Awake()
        {
            var white = HudRoot.White();
            for (int i = 0; i < Count; i++)
            {
                var pos = new Vector2(Margin + i * (Size + Gap), -Margin);
                pips[i] = HudRoot.NewImage(transform, "Pip" + i, white, Lit, new Vector2(0, 1), pos, new Vector2(Size, Size));
            }
        }

        void OnEnable() => GameEvents.HpChanged += OnHp;
        void OnDisable() => GameEvents.HpChanged -= OnHp;

        void OnHp(int hp)
        {
            for (int i = 0; i < Count; i++) pips[i].color = i < hp ? Lit : Unlit;
        }
    }
}
