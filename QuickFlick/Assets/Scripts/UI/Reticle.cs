using QuickFlick.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    public class Reticle : MonoBehaviour
    {
        const float CircleSize = 30f, DotSize = 4f;
        static readonly Color Hit = new Color(1f, 0.15f, 0.15f);

        Image circle;
        UnityEngine.Camera cam;

        void Awake()
        {
            var mid = new Vector2(0.5f, 0.5f);
            circle = HudRoot.NewImage(transform, "Circle", HudRoot.Ring(), Color.white, mid, Vector2.zero, new Vector2(CircleSize, CircleSize));
            HudRoot.NewImage(transform, "Dot", HudRoot.White(), Color.white, mid, Vector2.zero, new Vector2(DotSize, DotSize));
        }

        void Update()
        {
            if (cam == null) cam = UnityEngine.Camera.main;
            if (cam == null) return;
            // Enemy colliders are triggers, so the query must opt in to hitting them.
            bool onEnemy = Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, 500f, ~0, QueryTriggerInteraction.Collide)
                           && hit.collider.GetComponentInParent<Enemy>() != null;
            circle.color = onEnemy ? Hit : Color.white;
        }
    }
}
