using System;

namespace QuickFlick.Input
{
    public static class PhoneAimMapper
    {
        // Equivalent angle in (-180, 180].
        public static float WrapDeg(float d)
        {
            d %= 360f;
            if (d > 180f) d -= 360f;
            else if (d <= -180f) d += 360f;
            return d;
        }

        public static float Map(float deltaDeg, float deadzoneDeg, float fullSpeedDeg, float minOut)
        {
            float a = Math.Abs(deltaDeg);
            if (a <= deadzoneDeg) return 0f;
            float t = fullSpeedDeg <= 0f ? 1f : Math.Min(1f, Math.Max(0f, (a - deadzoneDeg) / fullSpeedDeg));
            float m = minOut + (1f - minOut) * t;
            return deltaDeg < 0f ? -m : m;
        }
    }
}
