namespace QuickFlick.Input
{
    public readonly struct PhoneReading
    {
        public readonly int Seq;
        public readonly float Alpha, Beta, Gamma, Ax, Ay, Az;
        public readonly int Recenter;

        public PhoneReading(int seq, float alpha, float beta, float gamma,
                            float ax, float ay, float az, int recenter)
        {
            Seq = seq; Alpha = alpha; Beta = beta; Gamma = gamma;
            Ax = ax; Ay = ay; Az = az; Recenter = recenter;
        }
    }
}
