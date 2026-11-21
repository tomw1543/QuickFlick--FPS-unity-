using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    // Builds the whole HUD canvas from code. Self-installs after scene load; no scene object needed.
    public class HudRoot : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (FindFirstObjectByType<HudRoot>() != null) return;
            new GameObject("HudRoot").AddComponent<HudRoot>();
        }

        public static HudRoot Instance;

        Image flashImage;
        float flashDuration, flashLeft;
        Color flashColor;

        public void Flash(Color c, float durationSeconds)
        {
            flashColor = c; flashDuration = flashLeft = durationSeconds;
        }

        void Update()
        {
            if (flashLeft <= 0f) return;
            flashLeft -= Time.unscaledDeltaTime;
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0.5f * Mathf.Clamp01(flashLeft / flashDuration));
        }

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // keeps the HUD out of GameBootstrap's scene cleanup
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Added first so it draws beneath the other HUD elements.
            flashImage = NewImage(transform, "Flash", White(), Color.clear, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var frt = flashImage.rectTransform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = frt.offsetMax = Vector2.zero;

            foreach (var t in new[] { typeof(Reticle), typeof(HpPips), typeof(ScoreText), typeof(WaveBanner), typeof(DebugOverlay), typeof(OffScreenArrows) })
            {
                var go = new GameObject(t.Name, typeof(RectTransform));
                go.transform.SetParent(transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
                go.AddComponent(t);
            }
        }

        // Shared UI builders. Anchor doubles as pivot so offsets read as insets from that corner.
        public static Image NewImage(Transform parent, string name, Sprite sprite, Color color, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = sprite; img.color = color; img.raycastTarget = false;
            return img;
        }

        public static Text NewText(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, TextAnchor align, int fontSize, Font font = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var t = go.AddComponent<Text>();
            t.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize; t.alignment = align; t.color = Color.white; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static Sprite White()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        public static Sprite Ring(int size = 64, float thickness = 4f)
        {
            var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
            float r = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    float a = Mathf.Clamp01(r - d) * Mathf.Clamp01(d - (r - thickness)); // 1px soft edges
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        // Upward-pointing filled triangle.
        public static Sprite Triangle(int size = 32)
        {
            var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float halfWidth = (1f - (y + 0.5f) / size) * size / 2f; // wide at the bottom, point at the top
                    float a = Mathf.Clamp01(halfWidth - Mathf.Abs(x + 0.5f - size / 2f) + 0.5f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
