using System.Globalization;
using System.Threading;
using UnityEngine;

namespace QuickFlick.Input
{
    public class SerialProtocolSelfCheck : MonoBehaviour
    {
        string fails = "";

        void Start()
        {
            var saved = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                Ok("0.123,-0.456,0.789", new Vector3(0.123f, -0.456f, 0.789f));
                Ok("  0.1 , 0.2 , 0.3  ", new Vector3(0.1f, 0.2f, 0.3f));
                Ok("1.5,2.5,3.5", new Vector3(1.5f, 2.5f, 3.5f)); // under de-DE thread culture
                foreach (var bad in new[] { "abc,1,2", "1,2", "1,2,3,4", "", "1,5,2,5,3,5" })
                    if (SerialProtocol.TryParseSample(bad, out _)) fails += $" '{bad}' should fail;";
            }
            finally { Thread.CurrentThread.CurrentCulture = saved; }

            if (fails.Length == 0) Debug.Log("[SerialProtocolSelfCheck] OK");
            else Debug.LogError("[SerialProtocolSelfCheck] FAIL:" + fails);
        }

        void Ok(string line, Vector3 want)
        {
            if (!SerialProtocol.TryParseSample(line, out var v) || (v - want).magnitude > 1e-4f)
                fails += $" '{line}' should parse to {want} (got {v});";
        }
    }
}
