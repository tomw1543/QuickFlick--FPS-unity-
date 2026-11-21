using System.Globalization;
using UnityEngine;

namespace QuickFlick.Input
{
    public static class SerialProtocol
    {
        // Line format from the Uno sketch: "x,y,z" floats in g.
        public static bool TryParseSample(string line, out Vector3 accel)
        {
            accel = default;
            if (string.IsNullOrWhiteSpace(line)) return false;
            var p = line.Split(',');
            if (p.Length != 3) return false;
            const NumberStyles s = NumberStyles.Float;
            var c = CultureInfo.InvariantCulture;
            if (!float.TryParse(p[0].Trim(), s, c, out var x) ||
                !float.TryParse(p[1].Trim(), s, c, out var y) ||
                !float.TryParse(p[2].Trim(), s, c, out var z)) return false;
            accel = new Vector3(x, y, z);
            return true;
        }
    }
}
