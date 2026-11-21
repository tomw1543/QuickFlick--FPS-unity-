namespace QuickFlick.Input
{
    // Fires once per push (accel along +Y above threshold); re-arms below release.
    public class PushDetector
    {
        readonly float threshold, release;
        readonly double cooldown;
        bool armed = true;
        double cooldownUntil;

        public PushDetector(float threshold, float release, double cooldownSeconds)
        {
            this.threshold = threshold;
            this.release = release;
            cooldown = cooldownSeconds;
        }

        public bool Feed(float accel, double timeSeconds)
        {
            if (accel < release) armed = true;
            if (armed && accel > threshold && timeSeconds >= cooldownUntil)
            {
                armed = false;
                cooldownUntil = timeSeconds + cooldown;
                return true;
            }
            return false;
        }

        public void Reset() { armed = true; cooldownUntil = 0; }
    }
}
