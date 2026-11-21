using System.Collections.Generic;
using QuickFlick.Enemies;
using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Audio
{
    public class Sfx : MonoBehaviour
    {
        public static Sfx Instance;

        const float Volume = 1f;
        static readonly (string key, string file)[] Clips =
        {
            ("cannon", "laserSmall_001"), ("mega", "laserLarge_002"), ("hit", "impactMetal_001"),
            ("death", "explosionCrunch_000"), ("playerhit", "forceField_002"),
        };

        readonly Dictionary<string, AudioClip> clips = new();
        AudioSource src;
        int lastHp = int.MaxValue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register() => new GameObject("Sfx").AddComponent<Sfx>();

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            foreach (var (key, file) in Clips)
            {
                var c = Resources.Load<AudioClip>("Audio/Sfx/" + file);
                if (c == null) Debug.LogWarning("Sfx: clip not found: " + file);
                else clips[key] = c;
            }
        }

        void OnEnable()
        {
            GameEvents.EnemyKilled += OnKilled;
            GameEvents.HpChanged += OnHp;
            GameEvents.ShotFired += OnShot;
            GameEvents.HitLanded += OnHit;
        }

        void OnDisable()
        {
            GameEvents.EnemyKilled -= OnKilled;
            GameEvents.HpChanged -= OnHp;
            GameEvents.ShotFired -= OnShot;
            GameEvents.HitLanded -= OnHit;
        }

        void OnKilled(Enemy e, bool wasMega) => Play("death");
        void OnShot(bool isMega) => Play(isMega ? "mega" : "cannon");
        void OnHit() => Play("hit");

        void OnHp(int hp)
        {
            if (hp < lastHp) Play("playerhit");
            lastHp = hp;
        }

        public void Play(string key)
        {
            if (clips.TryGetValue(key, out var c)) src.PlayOneShot(c, Volume);
        }
    }
}
