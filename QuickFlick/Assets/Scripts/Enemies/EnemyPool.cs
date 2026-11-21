using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuickFlick.Enemies
{
    public class EnemyPool : MonoBehaviour
    {
        public static EnemyPool Instance;

        static Mesh droneMesh;
        static Mesh DroneMesh => droneMesh != null ? droneMesh : droneMesh = BuildTetrahedron(); // lazy: Mesh can't be created in a static initializer
        static Material droneMat;

        readonly Dictionary<Type, Queue<Enemy>> free = new();
        readonly HashSet<Enemy> active = new();

        void Awake() => Instance = this;

        public T Get<T>() where T : Enemy
        {
            if (!free.TryGetValue(typeof(T), out var q)) free[typeof(T)] = q = new Queue<Enemy>();
            var e = q.Count > 0 ? q.Dequeue() : Build<T>();
            e.gameObject.SetActive(true);
            active.Add(e);
            return (T)e;
        }

        public void ReturnAll()
        {
            foreach (var e in new List<Enemy>(active)) Return(e);
        }

        public void Return(Enemy e)
        {
            if (!active.Remove(e)) return;
            e.gameObject.SetActive(false);
            if (!free.TryGetValue(e.GetType(), out var q)) free[e.GetType()] = q = new Queue<Enemy>();
            q.Enqueue(e);
        }

        Enemy Build<T>() where T : Enemy
        {
            var go = new GameObject(typeof(T).Name);
            go.transform.SetParent(transform, false);
            // Only Drone exists; add a branch per new enemy type here.
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            go.AddComponent<MeshFilter>().sharedMesh = DroneMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = DroneMaterial();
            go.AddComponent<SphereCollider>().isTrigger = true; // default radius 0.5
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true; // kinematic body so trigger events fire
            rb.useGravity = false;
            return go.AddComponent<T>();
        }

        static Material DroneMaterial()
        {
            if (droneMat != null) return droneMat;
            ColorUtility.TryParseHtmlString("#FF2E5A", out var c);
            droneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            droneMat.SetColor("_BaseColor", c);
            droneMat.EnableKeyword("_EMISSION");
            droneMat.SetColor("_EmissionColor", c * 2f);
            return droneMat;
        }

        static Mesh BuildTetrahedron()
        {
            var apex = new Vector3(0f, 0f, 0.5f);
            var top = new Vector3(0f, 0.5f, -0.5f);
            var left = new Vector3(-0.433f, -0.25f, -0.5f);
            var right = new Vector3(0.433f, -0.25f, -0.5f);
            var faces = new[] { (apex, top, left), (apex, right, top), (apex, left, right), (top, right, left) };

            var verts = new Vector3[12];
            var norms = new Vector3[12];
            var tris = new int[12];
            for (int i = 0; i < 4; i++)
            {
                var (a, b, c) = faces[i];
                var n = Vector3.Cross(b - a, c - a).normalized;
                // Unity front face is clockwise; flip any face whose normal points inward (centroid is the origin-ish).
                if (Vector3.Dot(n, (a + b + c) / 3f) < 0f) { (b, c) = (c, b); n = -n; }
                verts[i * 3] = a; verts[i * 3 + 1] = b; verts[i * 3 + 2] = c;
                norms[i * 3] = norms[i * 3 + 1] = norms[i * 3 + 2] = n;
                tris[i * 3] = i * 3; tris[i * 3 + 1] = i * 3 + 1; tris[i * 3 + 2] = i * 3 + 2;
            }
            var m = new Mesh { name = "DroneTetra", vertices = verts, normals = norms, triangles = tris };
            m.RecalculateBounds();
            return m;
        }
    }
}
