using NUnit.Framework;
using QuickFlick.Net;

namespace QuickFlick.Tests
{
    public class RelayUrlsTests
    {
        [TestCase("tom-7f3kq", true)]
        [TestCase("abc", true)]
        [TestCase("ab", false)]
        [TestCase("Tom-7f3kq", false)]
        [TestCase("a b c", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsValidRoom(string room, bool expected) => Assert.AreEqual(expected, RelayUrls.IsValidRoom(room));

        [Test]
        public void GameSocketUrl_Https() =>
            Assert.AreEqual("wss://x.workers.dev/ws?room=abc&role=game", RelayUrls.GameSocketUrl("https://x.workers.dev/", "abc"));

        [Test]
        public void GameSocketUrl_Http() =>
            Assert.AreEqual("ws://localhost:8787/ws?room=dev-room&role=game", RelayUrls.GameSocketUrl("http://localhost:8787", "dev-room"));

        [Test]
        public void GameSocketUrl_Bad()
        {
            Assert.IsNull(RelayUrls.GameSocketUrl("ftp://x", "abc"));
            Assert.IsNull(RelayUrls.GameSocketUrl("https://x", "A"));
            Assert.IsNull(RelayUrls.GameSocketUrl(null, "abc"));
        }

        [Test]
        public void JoinUrl() =>
            Assert.AreEqual("https://x.workers.dev/?room=abc", RelayUrls.JoinUrl("https://x.workers.dev//", "abc"));
    }
}
