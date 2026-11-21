using System.Collections;
using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        const string Track = "serhii_kliets-spaceship-arcade-shooter-game-background-soundtrack-318508"; // swap here
        const string Wave3Track = "jjk-divergent-fist";
        const int SwitchOnWave = 3;
        const float CrossfadeSeconds = 1.5f;
        const float Volume = 0.4f;

        AudioSource _active, _idle;
        AudioClip _default, _wave3;
        Coroutine _fade;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register() => new GameObject("MusicPlayer").AddComponent<MusicPlayer>();

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _active = NewSource();
            _idle = NewSource();
            _default = Resources.Load<AudioClip>("Audio/Music/" + Track);
            _wave3 = Resources.Load<AudioClip>("Audio/Music/" + Wave3Track);
            if (_default == null) Debug.LogWarning("MusicPlayer: clip not found: " + Track);
            if (_wave3 == null) Debug.LogWarning("MusicPlayer: clip not found: " + Wave3Track);
            if (_default == null) return;
            _active.clip = _default;
            _active.volume = Volume;
            _active.Play();
        }

        void OnEnable()
        {
            GameEvents.WaveStart += OnWaveStart;
            GameEvents.Restart += OnRestart;
        }

        void OnDisable()
        {
            GameEvents.WaveStart -= OnWaveStart;
            GameEvents.Restart -= OnRestart;
        }

        AudioSource NewSource()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.loop = true;
            s.playOnAwake = false;
            s.volume = 0f;
            return s;
        }

        void OnWaveStart(int wave) { if (wave == SwitchOnWave) CrossfadeTo(_wave3); }
        void OnRestart() => CrossfadeTo(_default);

        void CrossfadeTo(AudioClip clip)
        {
            if (clip == null || _active.clip == clip) return;
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(Fade(clip));
        }

        IEnumerator Fade(AudioClip clip)
        {
            // Swap roles; the old active source fades out from wherever it is.
            (_active, _idle) = (_idle, _active);
            _active.clip = clip;
            _active.volume = 0f;
            _active.Play();
            float outStart = _idle.volume;
            for (float t = 0f; t < CrossfadeSeconds; t += Time.unscaledDeltaTime)
            {
                float k = t / CrossfadeSeconds;
                _active.volume = Volume * k;
                _idle.volume = outStart * (1f - k);
                yield return null;
            }
            _active.volume = Volume;
            _idle.volume = 0f;
            _idle.Stop();
            _fade = null;
        }
    }
}
