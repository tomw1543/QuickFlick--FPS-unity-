using QuickFlick.UI;
using UnityEngine;

namespace QuickFlick.Input
{
    // Owns Mouse/Serial/Phone. Exactly one drives the game; 7/8/9 switch.
    public class InputSelector : MonoBehaviour, IControllerInput
    {
        MouseInput mouse;
        SerialInput serial;
        PhoneInput phone;
        IControllerInput active;
        string activeName;

        public string ActiveName => activeName;
        public Vector2 LeftStick => active.LeftStick;
        public Vector2 RightStick => active.RightStick;
        public bool LeftFire => active.LeftFire;
        public bool RightFire => active.RightFire;

        void Awake()
        {
            // SerialInput opens its port synchronously in OnEnable, so IsOpen is valid right after AddComponent.
            serial = gameObject.AddComponent<SerialInput>();
            mouse = gameObject.AddComponent<MouseInput>();
            if (serial.IsOpen)
            {
                mouse.enabled = false;
                active = serial; activeName = "Arduino";
                Debug.Log($"[Bootstrap] Input: Serial ({serial.PortName})");
            }
            else
            {
                serial.enabled = false;
                active = mouse; activeName = "Mouse";
                Debug.Log("[Bootstrap] Input: Mouse (serial unavailable)");
            }
            phone = gameObject.AddComponent<PhoneInput>(); // stays enabled so it keeps draining

            var panel = gameObject.AddComponent<PhoneDebugPanel>();
            panel.selector = this;
            panel.phone = phone;
        }

        void Update()
        {
            if (Pressed(KeyCode.Alpha7, KeyCode.Keypad7)) SwitchToMouse();
            else if (Pressed(KeyCode.Alpha8, KeyCode.Keypad8)) SwitchToSerial();
            else if (Pressed(KeyCode.Alpha9, KeyCode.Keypad9)) SwitchToPhone();
        }

        static bool Pressed(KeyCode a, KeyCode b) =>
            UnityEngine.Input.GetKeyDown(a) || UnityEngine.Input.GetKeyDown(b);

        void SwitchToMouse()
        {
            if (active == (IControllerInput)mouse) return;
            Deactivate();
            mouse.enabled = true;
            Activate(mouse, "Mouse");
        }

        void SwitchToSerial()
        {
            if (active == (IControllerInput)serial) return;
            serial.enabled = true;
            if (!serial.IsOpen)
            {
                serial.enabled = false;
                Notify("Arduino controller not available");
                return;
            }
            Deactivate();
            Activate(serial, "Arduino");
        }

        void SwitchToPhone()
        {
            if (phone.Replaced) { phone.RestartRelay(); Notify("Reconnecting phone relay…"); return; }
            if (active == (IControllerInput)phone) return;
            if (phone.ConfigError != null) { Notify("Phone not configured (.env)"); return; }
            if (!phone.IsAvailable) { Notify("No phone connected"); return; }
            Deactivate();
            Activate(phone, "Phone");
        }

        // Mouse disabling unlocks the cursor; serial disabling closes its port. Phone is never disabled.
        void Deactivate()
        {
            if (active == (IControllerInput)mouse) mouse.enabled = false;
            else if (active == (IControllerInput)serial) serial.enabled = false;
        }

        void Activate(IControllerInput device, string name)
        {
            active = device; activeName = name;
            Notify("Input: " + name);
        }

        static void Notify(string msg)
        {
            Debug.Log("[Input] " + msg);
            InputMessage.Show(msg);
        }
    }
}
