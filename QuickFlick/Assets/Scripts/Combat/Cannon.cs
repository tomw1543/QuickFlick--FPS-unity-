using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Combat
{
    public class Cannon : MonoBehaviour
    {
        public Transform muzzle;
        public Vector3 muzzleOffset;
        public float cooldownSeconds = Settings.CannonCooldownSeconds;

        bool prevHeld;
        float lastFire = float.NegativeInfinity;

        /// <summary>
        /// Returns true if the fire press was consumed. When megaShotFlag is set the
        /// MegaShotDetector already resolved the AOE, so no projectile spawns.
        /// </summary>
        public bool TryFire(bool fireHeld, bool megaShotFlag)
        {
            bool rising = fireHeld && !prevHeld;
            prevHeld = fireHeld;
            if (GameState.Instance != null && GameState.Instance.Current == GameState.State.Menu) return false;
            if (!rising || Time.time - lastFire < cooldownSeconds) return false;

            lastFire = Time.time;
            if (megaShotFlag) return true;

            var origin = muzzle.position + muzzle.TransformDirection(muzzleOffset);
            ProjectilePool.Instance.Get().Launch(origin, muzzle.forward, Settings.CannonDamage, PointLevel.NextShotColor());
            GameEvents.RaiseShotFired(false);
            return true;
        }
    }
}
