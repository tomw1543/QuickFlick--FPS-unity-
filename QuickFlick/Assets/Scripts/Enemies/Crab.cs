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
        const string ModelPath = "SkyBox/Converted/Enemy Large by Quaternius - UgBmRVnQ9h";
        static readonly Color CrabColor = new Color(0.55f, 0.20f, 0.15f);

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
            QuickFlick.Bootstrap.GameBootstrap.FixImportedMaterials(go, CrabColor);
        }
    }
}
