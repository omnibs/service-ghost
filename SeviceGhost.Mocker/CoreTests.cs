namespace SeviceGhost.Mocker
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using ServiceGhost.Core;

    [TestClass]
    public class CoreTests
    {
        [TestMethod]
        public void DisabledWait()
        {
            var original = new Clock();
            var fake = GhostHelper.CreateFake(original);

            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 30; i++)
            {
                fake.GetTime();
            }
            watch.Stop();
            var originalElapsed = watch.ElapsedMilliseconds;

            watch.Restart();
            GhostHelper.Mode = FakerModeEnum.Load;
            for (int i = 0; i < 30; i++)
            {
                fake.GetTime();
            }
            watch.Stop();
            var fakeElapsed = watch.ElapsedMilliseconds;

            Assert.IsTrue(originalElapsed > fakeElapsed);
        }

        public class Clock
        {
            public DateTime GetTime()
            {
                Thread.Sleep(50);
                return DateTime.Now;
            }
        }
    }
}