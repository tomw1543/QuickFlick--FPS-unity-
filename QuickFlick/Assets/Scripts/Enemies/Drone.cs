using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Enemies
{
    public class Drone : Enemy
    {
        const float ReachDistance = 1.5f;
        Transform player;

        public override void Reset(Vector3 position)
        {
            hp = Settings.DroneHp;
            transform.position = position;
            player = UnityEngine.Camera.main.transform;
            gameObject.SetActive(true);
        }

        public override void Tick(float dt)
        {
            if (player == null) return;
            transform.LookAt(player);
            transform.position += transform.forward * (Settings.DroneSpeed * dt);
            if (Vector3.Distance(transform.position, player.position) < ReachDistance)
            {
                if (PlayerHp.Instance != null) PlayerHp.Instance.TakeDamage(Settings.DroneDamage);
                Die();
            }
        }
    }
}
