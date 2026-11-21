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
        // Called BEFORE pool return, both when killed (killed=true, Die) and far-despawned (killed=false).
        // Base spawns the debris burst on kill; overrides should call base.
        protected virtual void OnBeforeDespawn(bool killed)
        {
            if (killed) Debris.Spawn(transform.position, BodyColor());
        }

        Color BodyColor()
        {
            var r = GetComponentInChildren<Renderer>();
            if (r == null || r.sharedMaterial == null) return Color.white;
            var m = r.sharedMaterial;
            if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
            if (m.HasProperty("_Color")) return m.GetColor("_Color");
            return Color.white;
        }

        protected virtual void Despawn()
        {
            OnBeforeDespawn(false);
            EnemyPool.Instance.Return(this);
        }

        public bool TakeDamage(float amount)
        {
            if (hp <= 0) return false; // already dead this frame
            hp -= amount;
            if (hp > 0) { Debris.Spawn(transform.position, BodyColor()); return false; }
            Die();
            return true;
        }

        protected virtual void Die()
        {
            OnBeforeDespawn(true);
            OnDeath?.Invoke(this);
            Systems.GameEvents.RaiseEnemyDied(this);
            EnemyPool.Instance.Return(this);
        }
    }
}
