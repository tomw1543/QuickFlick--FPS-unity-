using QuickFlick.Enemies;
using UnityEngine;

namespace QuickFlick.Systems
{
    public class Score : MonoBehaviour
    {
        public int Current { get; private set; }

        void OnEnable()
        {
            GameEvents.EnemyKilled += OnKilled;
            GameEvents.WaveClear += OnWaveClear;
            GameEvents.Restart += OnRestart;
        }

        void OnDisable()
        {
            GameEvents.EnemyKilled -= OnKilled;
            GameEvents.WaveClear -= OnWaveClear;
            GameEvents.Restart -= OnRestart;
        }

        void OnKilled(Enemy e, bool wasMega) => Add(Settings.DroneKillPoints * (wasMega ? Settings.MegaShotKillMultiplier : 1), wasMega ? "mega kill" : "kill");
        void OnWaveClear(int wave) => Add(Settings.WaveClearBonusPerWave * wave, $"wave {wave} bonus");
        void OnRestart() { Current = 0; Debug.Log("Score reset: 0"); GameEvents.RaiseScoreChanged(Current); }

        void Add(int points, string why)
        {
            Current += points;
            GameEvents.RaiseScoreChanged(Current);
            Debug.Log($"Score +{points} ({why}) = {Current}");
        }
    }
}
