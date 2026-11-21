using System.Collections;
using QuickFlick.Enemies;
using UnityEngine;

namespace QuickFlick.Systems
{
    // Bonus Mario: from wave 5, 20% chance per wave to fly in at a random moment. Killing him pays out in Score.
    public class MarioSpawner : MonoBehaviour
    {
        const int MinWave = 5;
        const float Chance = 0.2f;
        const float MinDelay = 2f, MaxDelay = 25f; // inside the 30s wave window, clear of the board wipe

        void OnEnable()
        {
            GameEvents.WaveStart += OnWaveStart;
            GameEvents.GameOver += StopPending;
            GameEvents.Restart += StopPending;
        }

        void OnDisable()
        {
            GameEvents.WaveStart -= OnWaveStart;
            GameEvents.GameOver -= StopPending;
            GameEvents.Restart -= StopPending;
        }

        void StopPending() => StopAllCoroutines();

        void OnWaveStart(int wave)
        {
            StopAllCoroutines(); // a late Mario from the previous wave would land after the board clear
            if (wave >= MinWave && Random.value < Chance) StartCoroutine(SpawnLater());
        }

        IEnumerator SpawnLater()
        {
            yield return new WaitForSeconds(Random.Range(MinDelay, MaxDelay));
            var cam = UnityEngine.Camera.main;
            if (cam == null || EnemyPool.Instance == null) yield break;
            var dir = Quaternion.Euler(-Random.Range(5f, 60f), Random.Range(0f, 360f), 0f) * Vector3.forward;
            EnemyPool.Instance.Get<Mario>().Reset(cam.transform.position + dir * Settings.SpawnRingRadius);
        }
    }
}
