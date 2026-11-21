using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Enemies
{
    public class Octopus : Enemy
    {
        // Spec: 6 HP, 2 dmg, speed 1.2 u/s = 1.0 tangential + 0.6 radial inward, collider 1.3
        const float TangentialSpeed = 1.0f;
        const float RadialSpeed = 0.6f;
        const int Damage = 2;
        const int MaxHp = 6;
        const float Scale = 1.8f;
        const float ColliderRadius = 1.3f;
        const float ReachDistance = 1.5f;
        const string ModelPath = "SkyBox/Converted/Spaceship_BarbaraTheBee";
        static readonly Color[] OctopusPalette = {
            new Color(0.95f, 0.85f, 0.40f), // yellow
            new Color(0.55f, 0.25f, 0.70f), // purple
        };

        Transform player;
        bool visualReady;

        // Pool entry point.
        public override void Reset(Vector3 position)
        {
            hp = MaxHp;
            player = UnityEngine.Camera.main.transform;
            transform.position = position;
            transform.localScale = Vector3.one * Scale;
            var sc = GetComponent<SphereCollider>();
            if (sc != null) sc.radius = ColliderRadius;
            AttachVisual();
            gameObject.SetActive(true);
        }

        public override void Tick(float dt)
        {
            if (player == null) return;

            var pos = transform.position;
            pos.y = player.position.y; // lock to the player's Y plane
            transform.position = pos;

            var to = player.position - transform.position;
            to.y = 0f;
            float r = to.magnitude;
            if (r > ReachDistance)
            {
                var n = to / r;
                transform.position += -n * (RadialSpeed * dt) + Vector3.Cross(Vector3.up, n) * (TangentialSpeed * dt);
            }

            var look = player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);

            if (Vector3.Distance(transform.position, player.position) < ReachDistance)
            {
                if (PlayerHp.Instance != null) PlayerHp.Instance.TakeDamage(Damage);
                Die();
            }
        }

        void AttachVisual()
        {
            if (visualReady) return;
            visualReady = true;
            var mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false; // hide pool placeholder tetrahedron
            var prefab = Resources.Load<GameObject>(ModelPath);
            if (prefab == null) { Debug.LogWarning("Octopus: FBX not found, keeping placeholder"); if (mr != null) mr.enabled = true; return; }
            var go = Instantiate(prefab, transform);
            go.transform.localPosition = Vector3.zero;
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);

            // Re-center the FBX so the billboard rotates around the mesh centroid, not the model's off-pivot origin.
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                var b = rends[0].bounds;
                for (int k = 1; k < rends.Length; k++) b.Encapsulate(rends[k].bounds);
                go.transform.localPosition = go.transform.localPosition - transform.InverseTransformPoint(b.center);
            }
            QuickFlick.Bootstrap.GameBootstrap.FixImportedMaterials(go, OctopusPalette[0]);

            // Override the uniform fallback per material slot so the model reads as multi-coloured.
            int i = 0;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int s = 0; s < mats.Length; s++)
                {
                    if (mats[s] == null) continue;
                    var tint = OctopusPalette[i++ % OctopusPalette.Length];
                    mats[s].SetColor("_BaseColor", tint);
                    mats[s].SetColor("_EmissionColor", tint * 0.4f); // match the emission default so brightness stays consistent
                }
            }

            // Diagnostic: lists the original renderers so odd parts can be identified by name.
            foreach (var r in go.GetComponentsInChildren<Renderer>())
                Debug.Log($"[Octopus] renderer={r.name} bounds={r.bounds.size}");

            // Hide the dome/bubble part that covers the model.
            string[] hideKeywords = { "sphere", "dome", "glass", "helmet", "bubble", "shield" };
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var n = r.name.ToLowerInvariant();
                foreach (var kw in hideKeywords)
                {
                    if (n.Contains(kw)) { r.enabled = false; break; }
                }
            }
        }
    }
}
