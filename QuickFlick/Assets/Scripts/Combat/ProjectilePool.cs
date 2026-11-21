using System.Collections.Generic;
using UnityEngine;

namespace QuickFlick.Combat
{
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance;

        readonly Queue<Projectile> free = new();
        readonly HashSet<Projectile> active = new();
        Shader lit;

        void Awake()
        {
            Instance = this;
            lit = Shader.Find("Universal Render Pipeline/Lit");
        }

        public Projectile Get()
        {
            var p = free.Count > 0 ? free.Dequeue() : Build();
            p.gameObject.SetActive(true);
            active.Add(p);
            return p;
        }

        public void Return(Projectile p)
        {
            if (!active.Remove(p)) return; // already returned (two triggers in one frame)
            p.gameObject.SetActive(false);
            free.Enqueue(p);
        }

        public void ReturnAll()
        {
            foreach (var p in new List<Projectile>(active)) Return(p);
        }

        Projectile Build()
        {
            var root = new GameObject("Projectile");
            root.transform.SetParent(transform, false);

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true; // kinematic body so trigger events fire
            rb.useGravity = false;

            // Primitive capsule is Y-long; rotate it onto root Z.
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            mesh.transform.SetParent(root.transform, false);
            mesh.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var col = mesh.GetComponent<CapsuleCollider>();
            col.isTrigger = true;
            // Forgiving hitbox: 2.5x the visible capsule (default radius 0.5, height 2); mesh scale unchanged.
            col.radius = 1.25f;
            col.height = 5f;
            mesh.GetComponent<Renderer>().material = new Material(lit);

            return root.AddComponent<Projectile>();
        }
    }
}
