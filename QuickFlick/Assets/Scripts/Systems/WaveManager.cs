using System.Collections;
using QuickFlick.Enemies;
using UnityEngine;

namespace QuickFlick.Systems
{
    public class WaveManager : MonoBehaviour
    {
        const float DelaySeconds = 5f; // pre-wave delay, doubles as the between-wave banner

        Spawner spawner;

        void OnEnable()
        {
            GameEvents.Restart += OnRestart;
        }

        void OnDisable()
        {
            GameEvents.Restart -= OnRestart;
        }

        void Start()
        {
            spawner = FindFirstObjectByType<Spawner>();
            StartCoroutine(Run());
        }

        void OnRestart()
        {
            StopAllCoroutines();
            spawner.SpawnBatch(0);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            for (int i = 0; ; i++)
            {
                yield return new WaitForSeconds(DelaySeconds);
                var def = Def(i);
                Debug.Log($"Wave {i + 1} START ({def.Count} drones, 1 per {def.SpawnInterval:0.00}s)");
                GameEvents.RaiseWaveStart(i + 1);
                spawner.SetRate(def.SpawnInterval);
                spawner.SpawnBatch(def.Count);
                // Wait until the spawner finished AND the pool has no live enemies.
                yield return new WaitForSeconds(def.Count * def.SpawnInterval);
                yield return new WaitUntil(() => QuickFlick.Enemies.EnemyPool.Instance != null && QuickFlick.Enemies.EnemyPool.Instance.ActiveCount == 0);
                Debug.Log($"Wave {i + 1} CLEAR + bonus {Settings.WaveClearBonusPerWave * (i + 1)}");
                GameEvents.RaiseWaveClear(i + 1);
            }
        }

        // Wave 4+: last table entry with +4 count and x0.9 interval per extra wave.
        static Settings.WaveDef Def(int i)
        {
            var w = Settings.Waves;
            if (i < w.Length) return w[i];
            int extra = i - w.Length + 1;
            var last = w[w.Length - 1];
            return new Settings.WaveDef(last.Count + 4 * extra, last.SpawnInterval * Mathf.Pow(0.9f, extra));
        }
    }
}
