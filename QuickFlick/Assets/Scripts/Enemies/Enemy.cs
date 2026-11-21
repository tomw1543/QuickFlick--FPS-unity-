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

        void Update() => Tick(Time.deltaTime);

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
