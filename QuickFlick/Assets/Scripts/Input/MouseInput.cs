using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Input
{
    public class MouseInput : MonoBehaviour, IControllerInput
    {
        Vector2 virtualTilt;

        public Vector2 LeftStick => virtualTilt;
        public Vector2 RightStick => virtualTilt;
        // Fully qualified: inside QuickFlick.Input, bare `Input` resolves to this namespace.
        public bool LeftFire => UnityEngine.Input.GetMouseButton(0);
        public bool RightFire => UnityEngine.Input.GetMouseButton(1);

        void OnEnable() => SetLocked(true);
        void OnDisable() => SetLocked(false);

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) SetLocked(false);
            else if (Cursor.lockState != CursorLockMode.Locked && UnityEngine.Input.GetMouseButtonDown(0)) SetLocked(true);

            if (Cursor.lockState != CursorLockMode.Locked) return;

            var delta = new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));
            virtualTilt += delta * Settings.MouseSensitivity;
            virtualTilt.x = Mathf.Clamp(virtualTilt.x, -1f, 1f);
            virtualTilt.y = Mathf.Clamp(virtualTilt.y, -1f, 1f);
        }

        static void SetLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
