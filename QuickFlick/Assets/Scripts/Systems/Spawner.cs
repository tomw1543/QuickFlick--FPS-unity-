using QuickFlick.Enemies;
using UnityEngine;

namespace QuickFlick.Systems
{
    public class Spawner : MonoBehaviour
    {
        float spawnInterval = 1.5f;
        bool spawning = true;
        int remainingToSpawn; // nothing spawns until WaveManager sets a batch
        float timer;

        public void SetRate(float interval) { spawnInterval = interval; }
        public void SetSpawning(bool on) { spawning = on; }
        public void SpawnBatch(int count) { remainingToSpawn = count; timer = 0f; spawning = true; }

        void Update()
        {
            if (!spawning || remainingToSpawn <= 0) return;
            timer += Time.deltaTime;
            if (timer < spawnInterval) return;
            timer = 0f;
            remainingToSpawn--;

            var playerPos = UnityEngine.Camera.main.transform.position;
            float yaw = Random.Range(0f, 360f);
            float pitch = Random.Range(5f, 60f);
            Vector3 dir = Quaternion.Euler(-pitch, yaw, 0f) * Vector3.forward;
            EnemyPool.Instance.Get<Drone>().Reset(playerPos + dir * Settings.SpawnRingRadius);
        }
    }
}
