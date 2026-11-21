using QuickFlick.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    // Draws the HP hearts. Class name kept so HudRoot's wiring is unchanged.
    public class HpPips : MonoBehaviour
    {
        const int Count = 5;
        const float Size = 32f, Gap = 8f, Margin = 20f;
        static readonly Color Lit = new Color(1f, 0.18f, 0.353f);   // #FF2E5A
        static readonly Color Unlit = new Color(0.2f, 0.2f, 0.2f);  // #333333
        static readonly Color HitFlash = new Color(1f, 0f, 0f, 0.5f);

        readonly Image[] hearts = new Image[Count];
        int prevHp = Count;

        void Awake()
        {
            var heart = HudRoot.Heart();
            for (int i = 0; i < Count; i++)
            {
                var pos = new Vector2(Margin + i * (Size + Gap), -Margin);
                hearts[i] = HudRoot.NewImage(transform, "Heart" + i, heart, Lit, new Vector2(0, 1), pos, new Vector2(Size, Size));
            }
        }

        void OnEnable() => GameEvents.HpChanged += OnHp;
        void OnDisable() => GameEvents.HpChanged -= OnHp;

        void OnHp(int hp)
        {
            for (int i = 0; i < Count; i++) hearts[i].color = i < hp ? Lit : Unlit;
            if (hp < prevHp && HudRoot.Instance != null) HudRoot.Instance.Flash(HitFlash, 0.4f);
            prevHp = hp;
        }
    }
}
