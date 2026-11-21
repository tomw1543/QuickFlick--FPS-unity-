using QuickFlick.Input;
using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Camera
{
    public class TurretCamera : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;

        float yaw;
        float pitch;

        void Awake()
        {
            // Fully qualified: inside QuickFlick.Camera, bare `Camera` resolves to this namespace.
            if (cameraTransform == null) cameraTransform = UnityEngine.Camera.main.transform;
        }

        public void Apply(StickCombiner.LookFrame frame)
        {
            var step = Time.deltaTime * Settings.CameraLookRateDegPerSec * frame.Speed;
            yaw += frame.Yaw * step;
            pitch = Mathf.Clamp(pitch + frame.Pitch * step, -Settings.PitchClampDegrees, Settings.PitchClampDegrees);
            cameraTransform.localRotation = Quaternion.Euler(-pitch, yaw, 0f);
        }
    }
}
