using System.Collections.Generic;
using System.Globalization;

namespace QuickFlick.Input
{
    public static class PhoneProtocol
    {
        // Control messages start with '#'. Unknown ones parse as None (ignored).
        public static bool TryParseControl(string msg, out PhoneControl c)
        {
            c = PhoneControl.None;
            if (msg == null) return false;
            var t = msg.Trim();
            if (t.Length == 0 || t[0] != '#') return false;
            if (t == "#phone-connected") c = PhoneControl.PhoneConnected;
            else if (t == "#phone-left") c = PhoneControl.PhoneLeft;
            return true;
        }

        // "seq,alpha,beta,gamma,ax,ay,az,recenter"
        public static bool TryParseSample(string s, out PhoneReading r)
        {
            r = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var p = s.Split(',');
            if (p.Length != 8) return false;
            var ci = CultureInfo.InvariantCulture;
            if (!int.TryParse(p[0].Trim(), NumberStyles.Integer, ci, out var seq) ||
                !int.TryParse(p[7].Trim(), NumberStyles.Integer, ci, out var rec)) return false;
            var f = new float[6];
            for (int i = 0; i < 6; i++)
            {
                if (!float.TryParse(p[i + 1].Trim(), NumberStyles.Float, ci, out f[i]) ||
                    float.IsNaN(f[i]) || float.IsInfinity(f[i])) return false;
            }
            r = new PhoneReading(seq, f[0], f[1], f[2], f[3], f[4], f[5], rec);
            return true;
        }

        public static int ParseSamples(string msg, List<PhoneReading> output)
        {
            if (string.IsNullOrWhiteSpace(msg) || msg.TrimStart()[0] == '#') return 0;
            int added = 0;
            foreach (var part in msg.Split(';'))
            {
                if (TryParseSample(part, out var r)) { output.Add(r); added++; }
            }
            return added;
        }
    }
}
