namespace QuickFlick.Systems
{
    public static class Settings
    {
        public const int PlayerHp = 5;
        public const float CannonCooldownSeconds = 0.15f;
        public const float MegaShotWindowSeconds = 0.15f;
        public const float MegaShotRange = 15f;
        public const float CannonDamage = 1f;
        public const float ProjectileSpeed = 60f;
        public const float ProjectileLifetime = 2f;

        public const int DroneHp = 1;
        public const float DroneSpeed = 2f;
        public const int DroneDamage = 1;

        public const int DroneKillPoints = 100;
        public const int MegaShotKillMultiplier = 3;
        public const int WaveClearBonusPerWave = 500;

        public const float SpawnRingRadius = 30f;
        public const float PitchClampDegrees = 70f;
        public const float MouseSensitivity = 0.03f;
        public const float StickDeadzone = 0.15f;
        public const float CameraLookRateDegPerSec = 120f;

        public readonly struct WaveDef
        {
            public readonly int Count;
            public readonly float SpawnInterval;
            public WaveDef(int count, float spawnInterval) { Count = count; SpawnInterval = spawnInterval; }
        }

        // Wave 4+: reuse the last entry, scaling Count up and SpawnInterval down (WaveManager's job).
        public static readonly WaveDef[] Waves = { new(5, 1.5f), new(8, 1.2f), new(12, 1.0f) };
    }
}
