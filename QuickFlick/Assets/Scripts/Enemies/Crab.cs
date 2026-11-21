using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Enemies
{
    public class Crab : Enemy
    {
        // SPEC values: 3 HP, 2 u/s, 1 dmg, collider 1.0
        const float Speed = 2f;
        const int Damage = 1;
        const int MaxHp = 3;
        const float Scale = 0.7f; // "little crabs" per user, tune 0.7-1.2 to taste
        const float ColliderRadius = 1.0f;
        const float ReachDistance = 1.5f;
        static readonly Vector3 RotateSpeedDeg = new Vector3(40f, 60f, 30f); // per-axis deg/sec; mismatched rates give a non-repeating tumble
        const string ModelPath = "SkyBox/Converted/Alien by Quaternius - HYUUkdugoP";
        static readonly Color[] CrabPalette = {
            new Color(0.55f, 0.20f, 0.15f), // deep red
            new Color(0.85f, 0.45f, 0.18f), // orange
            new Color(0.95f, 0.85f, 0.40f), // yellow
            new Color(0.25f, 0.55f, 0.55f), // teal
            new Color(0.90f, 0.75f, 0.55f), // cream
            new Color(0.45f, 0.25f, 0.55f), // violet
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
            var dir = (player.position - transform.position).normalized;
            transform.position += dir * (Speed * dt);
            transform.Rotate(RotateSpeedDeg * dt, Space.World);
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
            if (prefab == null) { Debug.LogWarning("Crab: FBX not found, keeping placeholder"); if (mr != null) mr.enabled = true; return; }
            var go = Instantiate(prefab, transform);
            go.transform.localPosition = Vector3.zero;
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);

            // Re-center the FBX so tumble rotates around the mesh centroid, not the model's off-pivot origin.
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                var b = rends[0].bounds;
                for (int k = 1; k < rends.Length; k++) b.Encapsulate(rends[k].bounds);
                go.transform.localPosition = go.transform.localPosition - transform.InverseTransformPoint(b.center);
            }
            QuickFlick.Bootstrap.GameBootstrap.FixImportedMaterials(go, CrabPalette[0]);

            // Override the uniform fallback per material slot so the alien reads as multi-coloured.
            int i = 0;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int s = 0; s < mats.Length; s++)
                {
                    if (mats[s] == null) continue;
                    var tint = CrabPalette[i++ % CrabPalette.Length];
                    mats[s].SetColor("_BaseColor", tint);
                    mats[s].SetColor("_EmissionColor", tint * 0.4f); // match the emission default so brightness stays consistent
                }
            }

            // Diagnostic: lists the original renderers so the dome can be identified by name if the hide below misses it.
            foreach (var r in go.GetComponentsInChildren<Renderer>())
                Debug.Log($"[Crab] renderer={r.name} bounds={r.bounds.size}");

            // Hide the dome/bubble part that covers the legs.
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
