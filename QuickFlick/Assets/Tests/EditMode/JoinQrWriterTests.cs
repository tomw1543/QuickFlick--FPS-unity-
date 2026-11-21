using NUnit.Framework;
using QuickFlick.Net;

namespace QuickFlick.Tests.EditMode
{
    public class JoinQrWriterTests
    {
        [Test]
        public void EncodePng_ProducesPng()
        {
            var png = JoinQrWriter.EncodePng("https://example.workers.dev/?room=dev-room");
            Assert.Greater(png.Length, 100);
            Assert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, new[] { png[0], png[1], png[2], png[3] });
        }
    }
}
