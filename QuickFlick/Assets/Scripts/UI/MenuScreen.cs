using QuickFlick.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    // Screen-space overlay. Any mouse click starts the game.
    public class MenuScreen : MonoBehaviour
    {
        GameObject overlay;
        RectTransform labelRt;

        void Awake()
        {
            var bg = HudRoot.NewImage(transform, "MenuOverlay", HudRoot.White(), new Color(0f, 0f, 0f, 0.6f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            overlay = bg.gameObject;
            var rt = bg.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var c = overlay.AddComponent<Canvas>();
            c.overrideSorting = true; c.sortingOrder = 200;

            var label = HudRoot.NewText(overlay.transform, "ClickToStart", new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1600, 200), TextAnchor.MiddleCenter, 96);
            label.text = "CLICK TO START";
            labelRt = label.rectTransform;
        }

        void Update()
        {
            if (overlay == null || !overlay.activeSelf) return;
            labelRt.localScale = Vector3.one * (1f + 0.1f * Mathf.Sin(Time.unscaledTime * 4f));
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (GameState.Instance != null) GameState.Instance.StartPlaying();
                overlay.SetActive(false);
            }
        }
    }
}
