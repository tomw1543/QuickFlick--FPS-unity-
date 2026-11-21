using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.UI
{
    // Brief centred-top message. Static Show() self-installs a hidden host.
    public class InputMessage : MonoBehaviour
    {
        static InputMessage instance;
        string text;
        float shownAt = -100f;

        public static void Show(string message)
        {
            if (instance == null)
            {
                var go = new GameObject("InputMessage") { hideFlags = HideFlags.HideInHierarchy };
                DontDestroyOnLoad(go);
                instance = go.AddComponent<InputMessage>();
            }
            instance.text = message;
            instance.shownAt = Time.unscaledTime;
        }

        void OnGUI()
        {
            float age = Time.unscaledTime - shownAt;
            float life = Settings.InputSwitchMessageSeconds;
            if (string.IsNullOrEmpty(text) || age > life) return;
            float alpha = Mathf.Clamp01((life - age) / 0.5f);
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            var style = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            const float w = 360f, h = 36f;
            GUI.Box(new Rect((Screen.width - w) / 2f, 20f, w, h), text, style);
            GUI.color = prev;
        }
    }
}
