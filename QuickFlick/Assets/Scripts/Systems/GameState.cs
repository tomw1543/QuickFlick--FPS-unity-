using QuickFlick.Combat;
using QuickFlick.Enemies;
using UnityEngine;

namespace QuickFlick.Systems
{
    public class GameState : MonoBehaviour
    {
        public enum State { Boot, Menu, Playing, GameOver }

        public static GameState Instance;
        public State Current { get; private set; } = State.Menu; // Menu from first frame so WaveManager/Cannon never see a gap

        public void StartPlaying() { if (Current == State.Menu) Current = State.Playing; }

        void Awake() => Instance = this;
        void OnEnable() => GameEvents.GameOver += OnGameOver;
        void OnDisable() => GameEvents.GameOver -= OnGameOver;

        void Start()
        {
            Time.timeScale = 1f; // stays in Menu until MenuScreen calls StartPlaying
        }

        void OnGameOver()
        {
            Current = State.GameOver;
            Time.timeScale = 0f;
            Debug.Log("GAME OVER - Space to restart");
        }

        void Update()
        {
            if (Current == State.GameOver && UnityEngine.Input.GetKeyDown(KeyCode.Space)) Restart();
        }

        // In-place reset. GameEvents.Reset() is not called: it would drop the systems' own subscriptions.
        void Restart()
        {
            Time.timeScale = 1f;
            Current = State.Playing;
            EnemyPool.Instance.ReturnAll();
            ProjectilePool.Instance.ReturnAll();
            GameEvents.RaiseRestart();
            Debug.Log("Restart");
        }
    }
}
