// Copyright © Jason Curl 2012-2020
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.Datastructures.TimerExpiryTest
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture(Category = "Datastructures/PassiveTimer")]
    public class TimerExpiryTest
    {
        [Test]
        public void TimerExpiry_Basic()
        {
            TimerExpiry te = new TimerExpiry(200);
            int t = te.RemainingTime();
            Assert.That(t, Is.GreaterThan(140), "Timer is less than 140ms (should be close to 200ms), remaining={0}", t);

            int[] rea = new int[1000];
            int c = 0;

            // Note, you should put your delays in loops like this one, as the OS doesn't guarantee that you will
            // actually wait this long. Normally, this kind of loop is exactly the type you want, as you wait for
            // another event to occur, and if it doesn't occur, you keep waiting until the timeout is zero.
            //
            // On some systems, a signal may cause a timeout to abort early also.
            int re = te.RemainingTime();
            do {
                rea[c++] = re;
                Thread.Sleep(re);
                re = te.RemainingTime();
            } while (re > 0);
            Assert.That(te.RemainingTime(), Is.EqualTo(0));

            for (int i = 0; i < c; i++) {
                Console.WriteLine("Wait {0}: {1}", i, rea[i]);
            }
        }

        [Test]
        public void TimerExpiry_Reset()
        {
            TimerExpiry te = new TimerExpiry(1000);
            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.GreaterThan(500));
            te.Reset();

            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.GreaterThan(500));
            te.Reset();

            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.GreaterThan(500));
            te.Reset();

            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.GreaterThan(500));
            te.Reset();

            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.GreaterThan(500));
            te.Reset();

            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.GreaterThan(500));
        }

        [Test]
        public void TimerExpiry_Reset2()
        {
            TimerExpiry te = new TimerExpiry(1000);
            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.GreaterThan(500));
            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.LessThan(500));

            te.Reset();
            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.GreaterThan(500));
            Thread.Sleep(350);
            Assert.That(te.RemainingTime(), Is.LessThan(500));
        }

        [Test]
        public void TimerExpiry_Negative()
        {
            TimerExpiry te = new TimerExpiry(-1);
            Assert.That(te.RemainingTime(), Is.EqualTo(Timeout.Infinite));
            Thread.Sleep(100);
            Assert.That(te.RemainingTime(), Is.EqualTo(Timeout.Infinite));

            TimerExpiry te2 = new TimerExpiry(100);
            Assert.That(te2.RemainingTime() > 0, Is.True);
            te2.Timeout = -1;
            Assert.That(te.RemainingTime(), Is.EqualTo(Timeout.Infinite));
            Thread.Sleep(400);
            Assert.That(te.RemainingTime(), Is.EqualTo(Timeout.Infinite));
        }

        [Test]
        public void TimerExpiry_Zero()
        {
            TimerExpiry te = new TimerExpiry(0);
            Assert.That(te.Expired, Is.True);
            Assert.That(te.RemainingTime(), Is.EqualTo(0));
        }
    }
}
