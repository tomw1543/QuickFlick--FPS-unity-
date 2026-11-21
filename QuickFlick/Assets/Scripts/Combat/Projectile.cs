using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Combat
{
    public class Projectile : MonoBehaviour
    {
        float speed = Settings.ProjectileSpeed;
        float lifetime;
        float damage;
        Vector3 direction;
        Material mat;

        void Awake() => mat = GetComponentInChildren<Renderer>().material;

        public void Launch(Vector3 origin, Vector3 direction, float damage, Color laserColor)
        {
            this.direction = direction.normalized;
            this.damage = damage;
            lifetime = Settings.ProjectileLifetime;

            transform.SetPositionAndRotation(origin, Quaternion.LookRotation(this.direction));
            // Root scale; the capsule mesh child is rotated so its long axis follows root Z.
            transform.localScale = new Vector3(0.18f, 0.18f, 1.0f);

            mat.SetColor("_BaseColor", laserColor);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", laserColor * 3f);
        }

        void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) ProjectilePool.Instance.Return(this);
        }

        void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null) return;
            bool killed = target.TakeDamage(damage);
            if (target is Enemies.Enemy) GameEvents.RaiseHitLanded();
            if (killed && target is Enemies.Enemy enemy) GameEvents.RaiseEnemyKilled(enemy, false);
            ProjectilePool.Instance.Return(this);
        }
    }
}
