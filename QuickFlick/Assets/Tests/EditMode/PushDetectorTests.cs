using NUnit.Framework;
using QuickFlick.Input;

namespace QuickFlick.Tests.EditMode
{
    public class PushDetectorTests
    {
        static PushDetector Make() => new PushDetector(1.5f, 0.5f, 0.3);

        [Test]
        public void BelowThresholdNeverFires()
        {
            var d = Make();
            for (int i = 0; i < 20; i++) Assert.IsFalse(d.Feed(1.4f, i));
        }

        [Test]
        public void CrossingFiresOnce_StayingHighDoesNot()
        {
            var d = Make();
            Assert.IsTrue(d.Feed(2f, 0));
            Assert.IsFalse(d.Feed(2f, 1));
            Assert.IsFalse(d.Feed(3f, 2));
        }

        [Test]
        public void MustDropBelowReleaseToRearm()
        {
            var d = Make();
            Assert.IsTrue(d.Feed(2f, 0));
            Assert.IsFalse(d.Feed(1.0f, 1)); // between release and threshold
            Assert.IsFalse(d.Feed(2f, 2));
            Assert.IsFalse(d.Feed(0.2f, 3));
            Assert.IsTrue(d.Feed(2f, 4));
        }

        [Test]
        public void CooldownBlocksThenAllows()
        {
            var d = Make();
            Assert.IsTrue(d.Feed(2f, 0));
            Assert.IsFalse(d.Feed(0.2f, 0.1));
            Assert.IsFalse(d.Feed(2f, 0.2)); // re-armed but inside cooldown
            Assert.IsFalse(d.Feed(0.2f, 0.25));
            Assert.IsTrue(d.Feed(2f, 0.4));
        }

        [Test]
        public void ResetRearms()
        {
            var d = Make();
            Assert.IsTrue(d.Feed(2f, 0));
            Assert.IsFalse(d.Feed(2f, 0.1));
            d.Reset();
            Assert.IsTrue(d.Feed(2f, 0.1));
        }
    }
}
