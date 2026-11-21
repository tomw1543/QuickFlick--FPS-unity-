using System.Collections.Generic;
using NUnit.Framework;
using QuickFlick.Input;

namespace QuickFlick.Tests.EditMode
{
    public class PhoneProtocolTests
    {
        [Test]
        public void TwoValidSamples()
        {
            var l = new List<PhoneReading>();
            int n = PhoneProtocol.ParseSamples("1,10.5,20,30,0.1,0.2,0.3,0;2,11,21,31,1,2,3,1", l);
            Assert.AreEqual(2, n);
            Assert.AreEqual(1, l[0].Seq);
            Assert.AreEqual(10.5f, l[0].Alpha, 1e-4f);
            Assert.AreEqual(0.3f, l[0].Az, 1e-4f);
            Assert.AreEqual(2, l[1].Seq);
            Assert.AreEqual(1, l[1].Recenter);
            Assert.AreEqual(31f, l[1].Gamma, 1e-4f);
        }

        [Test]
        public void BadSampleSkipped()
        {
            var l = new List<PhoneReading>();
            Assert.AreEqual(1, PhoneProtocol.ParseSamples("bad;2,1,2,3,4,5,6,0", l));
        }

        [Test]
        public void GarbageEmptyNullGiveZero()
        {
            var l = new List<PhoneReading>();
            Assert.AreEqual(0, PhoneProtocol.ParseSamples("garbage", l));
            Assert.AreEqual(0, PhoneProtocol.ParseSamples("", l));
            Assert.AreEqual(0, PhoneProtocol.ParseSamples(null, l));
            Assert.AreEqual(0, PhoneProtocol.ParseSamples("#phone-connected", l));
        }

        [Test]
        public void CommaDecimalsRejected()
        {
            // European "1,5" decimals add fields, so the count is wrong.
            Assert.IsFalse(PhoneProtocol.TryParseSample("1,0,5,2,3,4,5,6,7,0", out _));
        }

        [Test]
        public void NaNAndInfinityRejected()
        {
            Assert.IsFalse(PhoneProtocol.TryParseSample("1,NaN,2,3,4,5,6,0", out _));
            Assert.IsFalse(PhoneProtocol.TryParseSample("1,Infinity,2,3,4,5,6,0", out _));
        }

        [Test]
        public void ControlMessages()
        {
            Assert.IsTrue(PhoneProtocol.TryParseControl("#phone-connected", out var c));
            Assert.AreEqual(PhoneControl.PhoneConnected, c);
            Assert.IsTrue(PhoneProtocol.TryParseControl(" #phone-left ", out c));
            Assert.AreEqual(PhoneControl.PhoneLeft, c);
            Assert.IsTrue(PhoneProtocol.TryParseControl("#x", out c));
            Assert.AreEqual(PhoneControl.None, c);
        }

        [Test]
        public void DataIsNotControl()
        {
            Assert.IsFalse(PhoneProtocol.TryParseControl("1,2,3,4,5,6,7,0", out _));
        }
    }
}
