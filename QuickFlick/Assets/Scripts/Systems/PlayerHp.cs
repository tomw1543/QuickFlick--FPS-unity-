using UnityEngine;

namespace QuickFlick.Systems
{
    public class PlayerHp : MonoBehaviour
    {
        public static PlayerHp Instance;
        public int Hp { get; private set; }

        void Awake() { Instance = this; Hp = Settings.PlayerHp; }
        void Start() => GameEvents.RaiseHpChanged(Hp); // HUD subscribes after Awake, so seed here
        void OnEnable() => GameEvents.Restart += OnRestart;
        void OnDisable() => GameEvents.Restart -= OnRestart;

        void OnRestart() { Hp = Settings.PlayerHp; Debug.Log($"HP reset: {Hp}"); GameEvents.RaiseHpChanged(Hp); }

        public void TakeDamage(int amount)
        {
            if (Hp <= 0) return; // several drones can land in one frame; raise GameOver once
            Hp -= amount;
            GameEvents.RaiseHpChanged(Hp);
            Debug.Log($"HP {Hp}/{Settings.PlayerHp}");
            if (Hp <= 0) GameEvents.RaiseGameOver();
        }
    }
}
