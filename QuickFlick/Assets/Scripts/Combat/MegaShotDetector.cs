using QuickFlick.Enemies;
using QuickFlick.Systems;
using QuickFlick.UI;
using UnityEngine;

namespace QuickFlick.Combat
{
    /// <summary>
    /// Call Tick once per frame before the cannons. When both fire buttons go down within
    /// the window, the AOE fires immediately and the Pending flags stay true for that one frame
    /// so the cannons skip their projectiles (GameBootstrap passes them to Cannon.TryFire).
    /// </summary>
    public class MegaShotDetector : MonoBehaviour
    {
        public bool LeftMegaPending { get; private set; }
        public bool RightMegaPending { get; private set; }

        bool prevL, prevR;
        float lastL = float.NegativeInfinity, lastR = float.NegativeInfinity;

        public void Tick(bool left, bool right)
        {
            float t = Time.time;
            bool edge = false;
            if (left && !prevL) { lastL = t; edge = true; }
            if (right && !prevR) { lastR = t; edge = true; }
            prevL = left; prevR = right;

            LeftMegaPending = RightMegaPending = false;
            if (!edge || Mathf.Abs(lastL - lastR) > Settings.MegaShotWindowSeconds) return;

            LeftMegaPending = RightMegaPending = true;
            lastL = lastR = float.NegativeInfinity; // one AOE per pair of presses
            FireAoe();
        }

        public void ConsumeLeft() => LeftMegaPending = false;
        public void ConsumeRight() => RightMegaPending = false;

        static void FireAoe()
        {
            GameEvents.RaiseShotFired(true);
            if (HudRoot.Instance != null) HudRoot.Instance.Flash(Color.white, 0.2f);

            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            float halfFov = cam.fieldOfView * 0.5f;
            foreach (var enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                var toEnemy = enemy.transform.position - cam.transform.position;
                if (toEnemy.magnitude > Settings.MegaShotRange) continue;
                if (Vector3.Angle(cam.transform.forward, toEnemy) > halfFov) continue;
                if (enemy.TakeDamage(999f)) GameEvents.RaiseEnemyKilled(enemy, true);
            }
        }
    }
}
