using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Enemies
{
    // Reusable behaviour: spawns levelled with the camera, walks flat at the player, yaw-only facing,
    // random speed per life, solid-colour model. Subclasses are just constants (see Mario.cs).
    public abstract class LevelApproachEnemy : Enemy
    {
        const float ReachDistance = 1.5f;
        const float SolidEmission = 0.15f; // low, avoids bloom blowout

        protected abstract int MaxHp { get; }
        protected abstract int Damage { get; }
        protected abstract float MinSpeed { get; }
        protected abstract float MaxSpeed { get; }
        protected abstract float ColliderRadius { get; }
        protected abstract string ModelPath { get; }
        protected abstract float ModelScale { get; }
        protected abstract float ModelYawOffset { get; }   // degrees, so the mesh front faces the player
        protected virtual float ModelTiltX { get; } = 0f;  // degrees about X, to stand up a model imported lying down
        protected abstract Color Colour { get; }

        Transform player;
        float speed;
        bool visualReady;

        public override void Reset(Vector3 position)
        {
            hp = MaxHp;
            player = UnityEngine.Camera.main.transform;
            speed = Random.Range(MinSpeed, MaxSpeed);

            var off = position - player.position;
            off.y = 0f;
            if (off.sqrMagnitude < 0.0001f)
            {
                var a = Random.Range(0f, Mathf.PI * 2f);
                off = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            }
            transform.position = player.position + off.normalized * Settings.SpawnRingRadius;
            transform.localScale = Vector3.one;
            var sc = GetComponent<SphereCollider>();
            if (sc != null) sc.radius = ColliderRadius;
            FaceAndMove(0f);
            AttachVisual();
            gameObject.SetActive(true);
        }

        public override void Tick(float dt)
        {
            if (player == null) return;
            FaceAndMove(dt);
            var d = player.position - transform.position;
            d.y = 0f;
            if (d.magnitude < ReachDistance)
            {
                if (PlayerHp.Instance != null) PlayerHp.Instance.TakeDamage(Damage);
                Die();
            }
        }

        void FaceAndMove(float dt)
        {
            var d = player.position - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(d.normalized, Vector3.up); // yaw only
            transform.position += d.normalized * (speed * dt);
        }

        void AttachVisual()
        {
            if (visualReady) return;
            visualReady = true;
            var mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false; // hide pool placeholder
            var prefab = Resources.Load<GameObject>(ModelPath);
            if (prefab == null) { Debug.LogWarning($"{GetType().Name}: FBX not found, keeping placeholder"); if (mr != null) mr.enabled = true; return; }
            var go = Instantiate(prefab, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(0f, ModelYawOffset, 0f) * Quaternion.Euler(ModelTiltX, 0f, 0f);
            go.transform.localScale = Vector3.one * ModelScale; // scale the child, not the root (collider stays in world units)
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);
            QuickFlick.Bootstrap.GameBootstrap.FixImportedMaterials(go, Colour);

            // Force a solid colour: FixImportedMaterials keeps the source material's own colour.
            foreach (var r in go.GetComponentsInChildren<Renderer>())
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    m.SetColor("_BaseColor", Colour);
                    m.SetTexture("_BaseMap", null);
                    m.SetColor("_EmissionColor", Colour * SolidEmission);
                }

            // Centre the model on the root so the hit sphere sits on the body (model pivot is not centred).
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length > 0)
            {
                var b = rs[0].bounds;
                for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
                go.transform.position += transform.position - b.center;
            }
        }
    }
}
