// $URL$
// $Id$
using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RJCP.Datastructures.TimerExpiryTest
{
    [TestClass]
    public class TimerExpiryTest
    {
        [TestMethod]
        [TestCategory("Datastructures/PassiveTimer")]
        public void TimerExpiry_Basic()
        {
            TimerExpiry te = new TimerExpiry(200);
            int t = te.RemainingTime();
            Assert.IsTrue(t > 140, "Timer is less than 140ms (should be close to 200ms), remaining=" + t.ToString());

            int[] rea = new int[1000];
            int c = 0;

            // Note, you should put your delays in loops like this one, as
            // the OS doesn't guarantee that you will actually wait this
            // long. Normally, this kind of loop is exactly the type you
            // want, as you wait for another event to occur, and if it 
            // doesn't occur, you keep waiting until the timeout is zero.
            //
            // On some systems, a signal may cause a timeout to abort
            // early also.
            int re = te.RemainingTime();
            do {
                rea[c++] = re;
                System.Threading.Thread.Sleep((int)re);
                re = te.RemainingTime();
            } while (re > 0);
            Assert.AreEqual(0, te.RemainingTime());

            for (int i = 0; i < c; i++) {
                System.Diagnostics.Trace.WriteLine("Wait " + i.ToString() + ": " + rea[i].ToString());
            }
        }

        [TestMethod]
        [TestCategory("Datastructures/PassiveTimer")]
        public void TimerExpiry_Reset()
        {
            TimerExpiry te = new TimerExpiry(1000);
            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() > 500);
            te.Reset();

            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() > 500);
            te.Reset();

            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() > 500);
            te.Reset();

            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() > 500);
            te.Reset();

            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() > 500);
            te.Reset();

            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() > 500);
        }

        [TestMethod]
        [TestCategory("Datastructures/PassiveTimer")]
        public void TimerExpiry_Reset2()
        {
            TimerExpiry te = new TimerExpiry(1000);
            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() > 500);
            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() < 500);

            te.Reset();
            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() > 500);
            System.Threading.Thread.Sleep(350);
            Assert.IsTrue(te.RemainingTime() < 500);
        }

        [TestMethod]
        [TestCategory("Datastructures/PassiveTimer")]
        public void TimerExpiry_Negative()
        {
            TimerExpiry te = new TimerExpiry(-1);
            Assert.AreEqual(-1, te.RemainingTime());
            System.Threading.Thread.Sleep(100);
            Assert.AreEqual(-1, te.RemainingTime());

            TimerExpiry te2 = new TimerExpiry(100);
            Assert.IsTrue(te2.RemainingTime() > 0);
            te2.Timeout = -1;
            Assert.AreEqual(-1, te.RemainingTime());
            System.Threading.Thread.Sleep(400);
            Assert.AreEqual(-1, te.RemainingTime());

        }

        [TestMethod]
        [TestCategory("Datastructures/PassiveTimer")]
        public void TimerExpiry_Zero()
        {
            TimerExpiry te = new TimerExpiry(0);
            Assert.IsTrue(te.Expired);
            Assert.AreEqual(0, te.RemainingTime());
        }
    }
}
