using System;
using System.Collections;
using QuickFlick.Enemies;
using UnityEngine;

namespace QuickFlick.Systems
{
    public class WaveManager : MonoBehaviour
    {
        const float DelaySeconds = 5f;    // pre-wave banner window
        const float WaveSeconds = 30f;    // fixed wave length

        // 5-wave cycle; repeats forever.
        static readonly (Type type, int count)[][] Pattern =
        {
            new[] { (typeof(Asteroid), 5) },
            new[] { (typeof(Crab),     5) },
            new[] { (typeof(Octopus),  5) },
            new[] { (typeof(Asteroid), 5), (typeof(Crab),    3) },
            new[] { (typeof(Asteroid), 5), (typeof(Octopus), 2) },
        };

        void OnEnable()  { GameEvents.Restart += OnRestart; }
        void OnDisable() { GameEvents.Restart -= OnRestart; }

        void Start() => StartCoroutine(Run());

        void OnRestart()
        {
            StopAllCoroutines();
            if (EnemyPool.Instance != null) EnemyPool.Instance.ReturnAll();
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            for (int i = 0; ; i++)
            {
                yield return new WaitForSeconds(DelaySeconds);
                var batch = Pattern[i % Pattern.Length];
                int total = 0; foreach (var (_, c) in batch) total += c;
                float interval = WaveSeconds / Mathf.Max(1, total);

                Debug.Log($"Wave {i + 1} START ({total} enemies over {WaveSeconds}s)");
                GameEvents.RaiseWaveStart(i + 1);
                while (DebugSpawner.WavesPaused) yield return null;

                var cam = UnityEngine.Camera.main;
                float elapsed = 0f;
                foreach (var (type, count) in batch)
                {
                    for (int k = 0; k < count; k++)
                    {
                        if (EnemyPool.Instance != null && cam != null)
                        {
                            float yaw = UnityEngine.Random.Range(0f, 360f);
                            float pitch = UnityEngine.Random.Range(5f, 60f);
                            var dir = Quaternion.Euler(-pitch, yaw, 0f) * Vector3.forward;
                            var e = EnemyPool.Instance.GetByType(type);
                            e.Reset(cam.transform.position + dir * Settings.SpawnRingRadius);
                        }
                        yield return new WaitForSeconds(interval);
                        elapsed += interval;
                    }
                }
                if (elapsed < WaveSeconds) yield return new WaitForSeconds(WaveSeconds - elapsed);
                if (EnemyPool.Instance != null) EnemyPool.Instance.ReturnAll();

                Debug.Log($"Wave {i + 1} CLEAR + bonus {Settings.WaveClearBonusPerWave * (i + 1)}");
                GameEvents.RaiseWaveClear(i + 1);
            }
        }
    }
}
