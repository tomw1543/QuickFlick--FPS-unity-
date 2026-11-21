using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Enemies
{
    public class Asteroid : Enemy
    {
        const float Speed = 2f;
        const int Damage = 1;
        const float BigScale = 1.5f;
        const float SmallScale = 0.7f;
        const float SplitAngle = 20f;
        const float ReachDistance = 1.5f;
        const string ModelPath = "SkyBox/Converted/Asteroid by Poly by Google - enaIlQWET9a";
        static readonly Color RockColor = new Color(0.45f, 0.42f, 0.38f);

        public bool IsBig = true;
        Transform player;
        Vector3 dir;
        bool visualReady;

        // Pool entry point: spawns a big asteroid heading at the player.
        public override void Reset(Vector3 position) => Init(true, position, null);

        public void Init(bool big, Vector3 position, Vector3? direction)
        {
            IsBig = big;
            hp = 1;
            player = UnityEngine.Camera.main.transform;
            transform.position = position;
            transform.localScale = Vector3.one * (big ? BigScale : SmallScale);
            dir = direction ?? (player.position - position).normalized;
            AttachVisual();
            gameObject.SetActive(true);
        }

        public override void Tick(float dt)
        {
            if (player == null) return;
            transform.position += dir * (Speed * dt);
            if (Vector3.Distance(transform.position, player.position) < ReachDistance)
            {
                if (PlayerHp.Instance != null) PlayerHp.Instance.TakeDamage(Damage);
                IsBig = false; // reaching the player never splits
                Die();
            }
        }

        protected override void Die()
        {
            if (IsBig)
            {
                // Spawn before base.Die so this instance is still active and the pool hands out fresh ones.
                for (int s = -1; s <= 1; s += 2)
                    EnemyPool.Instance.Get<Asteroid>().Init(false, transform.position, Quaternion.AngleAxis(s * SplitAngle, Vector3.up) * dir);
            }
            base.Die();
        }

        // One-time per pooled instance. The pool builds every enemy with the drone tetra; hide it and use the FBX.
        void AttachVisual()
        {
            if (visualReady) return;
            visualReady = true;
            var mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
            var prefab = Resources.Load<GameObject>(ModelPath);
            if (prefab == null) { Debug.LogWarning("Asteroid: FBX not found, keeping placeholder mesh"); if (mr != null) mr.enabled = true; return; }
            var go = Instantiate(prefab, transform);
            go.transform.localPosition = Vector3.zero;
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c); // root SphereCollider handles hits
            QuickFlick.Bootstrap.GameBootstrap.FixImportedMaterials(go, RockColor);
        }
    }
}
