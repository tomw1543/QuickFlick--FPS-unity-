using UnityEngine;
using QuickFlick.Enemies;

namespace QuickFlick.Systems
{
    // F1..F4 force-spawn one enemy of each type at a random point on the spawn ring.
    // Logs a note if a type isn't implemented yet, so teammates can test their own class
    // without touching Spawner.cs or WaveManager.cs.
    public class DebugSpawner : MonoBehaviour
    {
        public static bool WavesPaused;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (FindFirstObjectByType<DebugSpawner>() == null) new GameObject("DebugSpawner").AddComponent<DebugSpawner>();
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject); // keeps the debug spawner out of GameBootstrap's scene cleanup
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1)) TrySpawn("Crab");
            if (UnityEngine.Input.GetKeyDown(KeyCode.F2)) TrySpawn("Octopus");
            if (UnityEngine.Input.GetKeyDown(KeyCode.F3)) TrySpawn("SpaceDrone");
            if (UnityEngine.Input.GetKeyDown(KeyCode.F4)) TrySpawn("Asteroid");
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                WavesPaused = !WavesPaused;
                Debug.Log($"[DebugSpawn] Waves {(WavesPaused ? "PAUSED" : "RESUMED")}");
            }
        }

        static void TrySpawn(string typeName)
        {
            var t = System.Type.GetType($"QuickFlick.Enemies.{typeName}, QuickFlick");
            if (t == null) { Debug.Log($"[DebugSpawn] {typeName} not implemented yet"); return; }
            if (EnemyPool.Instance == null) { Debug.Log("[DebugSpawn] EnemyPool not ready"); return; }

            var yaw = Random.Range(0f, 360f);
            var pitch = Random.Range(-30f, 60f);
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            var dir = Quaternion.Euler(-pitch, yaw, 0f) * Vector3.forward;
            var pos = cam.transform.position + dir * Settings.SpawnRingRadius;

            var enemy = EnemyPool.Instance.GetByType(t);
            enemy.Reset(pos);
            Debug.Log($"[DebugSpawn] {typeName} at {pos}");
        }
    }
}
