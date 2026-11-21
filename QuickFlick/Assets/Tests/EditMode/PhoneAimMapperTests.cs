using NUnit.Framework;
using QuickFlick.Input;

namespace QuickFlick.Tests.EditMode
{
    public class PhoneAimMapperTests
    {
        [Test]
        public void WrapDeg()
        {
            Assert.AreEqual(-1f, PhoneAimMapper.WrapDeg(359f), 1e-4f);
            Assert.AreEqual(179f, PhoneAimMapper.WrapDeg(-181f), 1e-4f);
            Assert.AreEqual(180f, PhoneAimMapper.WrapDeg(180f), 1e-4f);
        }

        [Test]
        public void InsideDeadzoneIsZero()
        {
            Assert.AreEqual(0f, PhoneAimMapper.Map(1.5f, 1.5f, 20f, 0.16f));
            Assert.AreEqual(0f, PhoneAimMapper.Map(-1f, 1.5f, 20f, 0.16f));
        }

        [Test]
        public void JustPastDeadzoneIsMinOut()
        {
            Assert.AreEqual(0.16f, PhoneAimMapper.Map(1.501f, 1.5f, 20f, 0.16f), 0.01f);
            Assert.AreEqual(-0.16f, PhoneAimMapper.Map(-1.501f, 1.5f, 20f, 0.16f), 0.01f);
        }

        [Test]
        public void FullSpeedAndClamp()
        {
            Assert.AreEqual(1f, PhoneAimMapper.Map(21.5f, 1.5f, 20f, 0.16f), 1e-4f);
            Assert.AreEqual(-1f, PhoneAimMapper.Map(-21.5f, 1.5f, 20f, 0.16f), 1e-4f);
            Assert.AreEqual(1f, PhoneAimMapper.Map(90f, 1.5f, 20f, 0.16f), 1e-4f);
            Assert.AreEqual(-1f, PhoneAimMapper.Map(-90f, 1.5f, 20f, 0.16f), 1e-4f);
        }

        [Test]
        public void ThreeDegreesAboveStickDeadzone()
        {
            Assert.Greater(PhoneAimMapper.Map(3f, 1.5f, 20f, 0.16f), 0.15f);
        }
    }
}
