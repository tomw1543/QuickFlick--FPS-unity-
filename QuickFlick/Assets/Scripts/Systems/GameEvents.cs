using System;
using QuickFlick.Enemies;
using UnityEngine;

namespace QuickFlick.Systems
{
    public static class GameEvents
    {
        public static event Action<Enemy, bool> EnemyKilled; // bool: killed by a MegaShot
        public static event Action<Enemy> EnemyDied;         // any death, incl. reaching the player
        public static event Action<int> WaveStart;
        public static event Action<int> WaveClear;
        public static event Action<int> ScoreChanged;
        public static event Action GameOver;
        public static event Action Restart;
        public static event Action<bool> ShotFired;          // bool: MegaShot
        public static event Action HitLanded;                // projectile damaged an enemy, kill or not
        public static event Action<int> HpChanged;           // new player HP

        public static void RaiseEnemyKilled(Enemy e, bool wasMega) => EnemyKilled?.Invoke(e, wasMega);
        public static void RaiseEnemyDied(Enemy e) => EnemyDied?.Invoke(e);
        public static void RaiseWaveStart(int wave) => WaveStart?.Invoke(wave);
        public static void RaiseWaveClear(int wave) => WaveClear?.Invoke(wave);
        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
        public static void RaiseGameOver() => GameOver?.Invoke();
        public static void RaiseRestart() => Restart?.Invoke();
        public static void RaiseShotFired(bool isMega) => ShotFired?.Invoke(isMega);
        public static void RaiseHitLanded() => HitLanded?.Invoke();
        public static void RaiseHpChanged(int hp) => HpChanged?.Invoke(hp);

        // Also runs on play-mode entry, so handlers never leak across sessions when domain reload is off.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            EnemyKilled = null; EnemyDied = null; WaveStart = null; WaveClear = null; GameOver = null; Restart = null;
            ScoreChanged = null; HpChanged = null; ShotFired = null; HitLanded = null;
        }
    }
}
