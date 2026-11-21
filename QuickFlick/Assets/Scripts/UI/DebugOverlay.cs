using QuickFlick.Enemies;
using QuickFlick.Input;
using QuickFlick.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    public class DebugOverlay : MonoBehaviour
    {
        Text text;
        float fps = 60f, nextFind;
        int score, hp, wave;
        IControllerInput input;

        void Awake()
        {
            text = HudRoot.NewText(transform, "Debug", new Vector2(0, 1), new Vector2(20, -50), new Vector2(500, 200), TextAnchor.UpperLeft, 16,
                Font.CreateDynamicFontFromOSFont("Consolas", 16));
            text.enabled = false;
        }

        void OnEnable() { GameEvents.ScoreChanged += OnScore; GameEvents.HpChanged += OnHp; GameEvents.WaveStart += OnWave; }
        void OnDisable() { GameEvents.ScoreChanged -= OnScore; GameEvents.HpChanged -= OnHp; GameEvents.WaveStart -= OnWave; }

        void OnScore(int s) => score = s;
        void OnHp(int h) => hp = h;
        void OnWave(int w) => wave = w;

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F3)) text.enabled = !text.enabled;
            fps = Mathf.Lerp(fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 1e-4f), 0.1f);
            if (!text.enabled) return;

            // No stick event exists; find the input component once (retry 1/s). Debug-only.
            if (input == null && Time.unscaledTime >= nextFind)
            {
                nextFind = Time.unscaledTime + 1f;
                foreach (var m in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                    if (m is IControllerInput i) { input = i; break; }
            }
            var l = input != null ? input.LeftStick : Vector2.zero;
            var r = input != null ? input.RightStick : Vector2.zero;
            int enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
            text.text = $"FPS    {fps:0}\nL      {l.x:0.00} {l.y:0.00}\nR      {r.x:0.00} {r.y:0.00}\nENEMY  {enemies}\nSCORE  {score}\nHP     {hp}\nWAVE   {wave}";
        }
    }
}
