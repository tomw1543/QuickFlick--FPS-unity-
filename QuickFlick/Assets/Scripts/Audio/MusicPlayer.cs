using UnityEngine;

namespace QuickFlick.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        const string Track = "serhii_kliets-spaceship-arcade-shooter-game-background-soundtrack-318508"; // swap here
        const float Volume = 0.4f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register() => new GameObject("MusicPlayer").AddComponent<MusicPlayer>();

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            var src = gameObject.AddComponent<AudioSource>();
            src.loop = true;
            src.volume = Volume;
            src.clip = Resources.Load<AudioClip>("Audio/Music/" + Track);
            if (src.clip == null) { Debug.LogWarning("MusicPlayer: clip not found: " + Track); return; }
            src.Play();
        }
    }
}
