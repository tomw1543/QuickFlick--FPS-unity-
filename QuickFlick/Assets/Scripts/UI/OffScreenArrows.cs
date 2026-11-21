using QuickFlick.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace QuickFlick.UI
{
    public class OffScreenArrows : MonoBehaviour
    {
        const int MaxArrows = 20;
        const float Inset = 50f, ArrowSize = 28f;
        static readonly Color Magenta = new Color(1f, 0.18f, 0.353f); // #FF2E5A

        readonly Image[] arrows = new Image[MaxArrows];
        UnityEngine.Camera cam;

        void Awake()
        {
            var tri = HudRoot.Triangle();
            for (int i = 0; i < MaxArrows; i++)
            {
                arrows[i] = HudRoot.NewImage(transform, "Arrow" + i, tri, Magenta, Vector2.zero, Vector2.zero, new Vector2(ArrowSize, ArrowSize));
                arrows[i].gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (cam == null) cam = UnityEngine.Camera.main;
            if (cam == null) return;
            float w = Screen.width, h = Screen.height;
            int used = 0;
            foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (used >= MaxArrows) break;
                var vp = cam.WorldToViewportPoint(e.transform.position);
                if (vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f) continue;

                // Offset from screen centre in pixels; behind the camera the projection is mirrored, so flip it.
                var dir = new Vector2((vp.x - 0.5f) * w, (vp.y - 0.5f) * h) * (vp.z < 0f ? -1f : 1f);
                if (dir.sqrMagnitude < 1e-4f) dir = Vector2.down; // dead behind
                float sx = Mathf.Abs(dir.x) > 1e-4f ? (w / 2f - Inset) / Mathf.Abs(dir.x) : float.MaxValue;
                float sy = Mathf.Abs(dir.y) > 1e-4f ? (h / 2f - Inset) / Mathf.Abs(dir.y) : float.MaxValue;
                var edge = dir * Mathf.Min(sx, sy) + new Vector2(w / 2f, h / 2f);

                var a = arrows[used++];
                a.gameObject.SetActive(true);
                var rt = a.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(edge.x / w, edge.y / h); // normalised anchors survive canvas scaling
                rt.anchoredPosition = Vector2.zero;
                rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
                float dist = Vector3.Distance(cam.transform.position, e.transform.position);
                float s = ArrowSize * Mathf.Clamp(20f / dist, 0.5f, 2.2f); // closer = bigger
                rt.sizeDelta = new Vector2(s, s);
            }
            for (int i = used; i < MaxArrows; i++) arrows[i].gameObject.SetActive(false);
        }
    }
}
