using QuickFlick.Combat;
using UnityEngine;

namespace QuickFlick.Enemies
{
    public abstract class Enemy : MonoBehaviour, IDamageable
    {
        protected float hp;
        public event System.Action<Enemy> OnDeath;

        public abstract void Tick(float dt);
        public abstract void Reset(Vector3 position);

        static UnityEngine.Camera cachedCam;

        void Update()
        {
            Tick(Time.deltaTime);
            if (cachedCam == null) cachedCam = UnityEngine.Camera.main;
            if (cachedCam != null && Vector3.Distance(transform.position, cachedCam.transform.position) > QuickFlick.Systems.Settings.EnemyDespawnDistance)
                Despawn();
        }

        // Pool return without death event or side effects (e.g. no asteroid split). Override only if cleanup differs.
        protected virtual void Despawn()
        {
            EnemyPool.Instance.Return(this);
        }

        public bool TakeDamage(float amount)
        {
            if (hp <= 0) return false; // already dead this frame
            hp -= amount;
            if (hp > 0) return false;
            Die();
            return true;
        }

        protected virtual void Die()
        {
            OnDeath?.Invoke(this);
            Systems.GameEvents.RaiseEnemyDied(this);
            EnemyPool.Instance.Return(this);
        }
    }
}
