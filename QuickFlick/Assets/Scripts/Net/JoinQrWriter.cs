using System.IO;
using Net.Codecrete.QrCodeGenerator;
using QuickFlick.Systems;
using UnityEngine;

namespace QuickFlick.Net
{
    // Main thread. Writes Tools/join-qr.png + join-url.txt next to the Assets folder.
    public static class JoinQrWriter
    {
        const int PixelsPerModule = 10;
        const int BorderModules = 4;

        public static byte[] EncodePng(string url)
        {
            var qr = QrCode.EncodeText(url, QrCode.Ecc.Medium);
            return qr.ToPngBitmap(BorderModules, PixelsPerModule);
        }

        public static void Write(string url)
        {
            try
            {
                var dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools"));
                Directory.CreateDirectory(dir);
                var png = Path.Combine(dir, "join-qr.png");
                var urlFile = Path.Combine(dir, "join-url.txt");
                string old = File.Exists(urlFile) ? File.ReadAllText(urlFile) : null;
                File.WriteAllBytes(png, EncodePng(url));
                File.WriteAllText(urlFile, url);
                if (Settings.PhoneOpenQrOnStart && old != url) Application.OpenURL("file:///" + png.Replace('\\', '/'));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Phone] could not write join QR: " + e.GetType().Name + ": " + e.Message);
            }
        }
    }
}
