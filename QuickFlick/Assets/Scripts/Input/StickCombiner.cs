using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Input
{
    public static class StickCombiner
    {
        public readonly struct LookFrame
        {
            public readonly float Yaw;
            public readonly float Pitch;
            public readonly float Speed;
            public LookFrame(float yaw, float pitch, float speed) { Yaw = yaw; Pitch = pitch; Speed = speed; }
        }

        public static LookFrame Combine(Vector2 left, Vector2 right)
        {
            if (left.magnitude < Settings.StickDeadzone) left = Vector2.zero;
            if (right.magnitude < Settings.StickDeadzone) right = Vector2.zero;
            var avg = (left + right) * 0.5f;
            var speed = 1f - (left - right).magnitude * 0.5f;
            return new LookFrame(avg.x, avg.y, speed < 0f ? 0f : speed > 1f ? 1f : speed);
        }
    }

#if UNITY_EDITOR
    static class StickCombinerSelfCheck
    {
        [UnityEditor.InitializeOnLoadMethod]
        static void Run()
        {
            var parallel = StickCombiner.Combine(Vector2.right, Vector2.right);
            Debug.Assert(Mathf.Approximately(parallel.Speed, 1f), "StickCombiner: parallel sticks should give Speed 1");
            Debug.Assert(Mathf.Approximately(parallel.Yaw, 1f), "StickCombiner: parallel (1,0) should give Yaw 1");
            var opposed = StickCombiner.Combine(Vector2.right, Vector2.left);
            Debug.Assert(Mathf.Approximately(opposed.Speed, 0f), "StickCombiner: opposing sticks should give Speed 0");
            var tiny = StickCombiner.Combine(new Vector2(0.1f, 0), new Vector2(0.1f, 0));
            Debug.Assert(Mathf.Approximately(tiny.Yaw, 0f), "StickCombiner: tiny tilt should be killed by deadzone");
            var zero =StickCombiner.Combine(Vector2.zero, Vector2.zero);
            Debug.Assert(Mathf.Approximately(zero.Speed, 1f), "StickCombiner: zero sticks agree, Speed 1");
        }
    }
#endif
}
