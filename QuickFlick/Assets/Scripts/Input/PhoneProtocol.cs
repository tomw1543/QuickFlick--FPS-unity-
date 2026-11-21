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
            if (!TryF(p[1], out var a) || !TryF(p[2], out var b) || !TryF(p[3], out var g) ||
                !TryF(p[4], out var ax) || !TryF(p[5], out var ay) || !TryF(p[6], out var az)) return false;
            r = new PhoneReading(seq, a, b, g, ax, ay, az, rec);
            return true;
        }

        static bool TryF(string s, out float f) =>
            float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out f) &&
            !float.IsNaN(f) && !float.IsInfinity(f);

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
