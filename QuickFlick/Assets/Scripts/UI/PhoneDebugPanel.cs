using QuickFlick.Input;
using UnityEngine;

namespace QuickFlick.UI
{
    // F6 toggles. Hidden by default.
    public class PhoneDebugPanel : MonoBehaviour
    {
        public InputSelector selector;
        public PhoneInput phone;
        bool visible;

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F6)) visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || phone == null) return;
            var p = phone;
            string text =
                $"Active input: {(selector != null ? selector.ActiveName : "?")}\n" +
                $"Config error: {p.ConfigError ?? "none"}\n" +
                $"Relay connected: {p.RelayConnected}   phone present: {p.PhonePresent}\n" +
                $"Relay error: {p.RelayError ?? "none"}\n" +
                $"Msgs/s: {p.MessagesPerSecond:F1}   available: {p.IsAvailable}\n" +
                $"alpha {p.LastAlpha:F1}  beta {p.LastBeta:F1}  gamma {p.LastGamma:F1}\n" +
                $"d-alpha {p.DeltaAlpha:F1}  d-beta {p.DeltaBeta:F1}\n" +
                $"stick ({p.LeftStick.x:F2}, {p.LeftStick.y:F2})\n" +
                $"ay {p.LastAy:F1}  peak {p.RecentPeakAy:F1}  pushes {p.PushCount}";
            const float w = 340f, h = 190f;
            GUI.Box(new Rect(Screen.width - w - 10f, 60f, w, h), text, new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 13 });
        }
    }
}
