using System.Collections.Generic;
using UnityEngine;

namespace QuickFlick.Combat
{
    // Pooled cube burst. Spawn() grabs one from the pool, Update() flies the cubes then returns it.
    public class Debris : MonoBehaviour
    {
        const int CubeCount = 10;
        const int MaxLive = 16;
        const float Lifetime = 0.6f;
        const float MinSpeed = 5f, MaxSpeed = 15f;
        const float CubeSize = 0.25f;

        static readonly Queue<Debris> pool = new Queue<Debris>();
        static int live;

        Transform[] cubes;
        Vector3[] vel, spin;
        Material mat;
        float age;

        public static void Spawn(Vector3 pos, Color color)
        {
            if (live >= MaxLive) return;
            var d = pool.Count > 0 ? pool.Dequeue() : Create();
            live++;
            d.Burst(pos, color);
        }

        static Debris Create()
        {
            var go = new GameObject("Debris");
            var d = go.AddComponent<Debris>();
            d.mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            d.mat.EnableKeyword("_EMISSION");
            d.cubes = new Transform[CubeCount];
            d.vel = new Vector3[CubeCount];
            d.spin = new Vector3[CubeCount];
            for (int i = 0; i < CubeCount; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(c.GetComponent<Collider>());
                c.GetComponent<Renderer>().sharedMaterial = d.mat;
                c.transform.SetParent(go.transform, false);
                d.cubes[i] = c.transform;
            }
            return d;
        }

        void Burst(Vector3 pos, Color color)
        {
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_EmissionColor", color);
            transform.position = pos;
            age = 0f;
            for (int i = 0; i < CubeCount; i++)
            {
                cubes[i].localPosition = Vector3.zero;
                cubes[i].localRotation = Random.rotation;
                cubes[i].localScale = Vector3.one * CubeSize;
                vel[i] = Random.onUnitSphere * Random.Range(MinSpeed, MaxSpeed);
                spin[i] = Random.insideUnitSphere * 360f;
            }
            gameObject.SetActive(true);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            if (age >= Lifetime)
            {
                gameObject.SetActive(false);
                live--;
                pool.Enqueue(this);
                return;
            }
            float s = CubeSize * (1f - age / Lifetime);
            for (int i = 0; i < CubeCount; i++)
            {
                cubes[i].localPosition += vel[i] * dt;
                cubes[i].Rotate(spin[i] * dt);
                cubes[i].localScale = Vector3.one * s;
            }
        }
    }
}
