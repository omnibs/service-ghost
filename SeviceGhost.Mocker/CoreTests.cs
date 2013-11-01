namespace SeviceGhost.Mocker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using ServiceGhost.Core;

    [TestClass]
    public class CoreTests
    {
        public void AddUser(UserService service, int count)
        {
            Assert.AreEqual(count, service.List().Count);
            service.Add("Paul");
            Assert.AreEqual(count +1, service.List().Count);
        }

        public void RemoveUser(UserService service, int count)
        {
            Assert.AreEqual(count, service.List().Count);
            service.Remove("Joe");
            Assert.AreEqual(count - 1, service.List().Count);
        }

        [TestMethod]
        public void SameContextSimple()
        {
            var fake = GhostHelper.CreateFake(new UserService());
            this.AddUser(fake, 1);

            GhostHelper.Mode = FakerModeEnum.Load;
            this.AddUser(fake, 1);
        }


        [TestMethod]
        public void NoRecordedDataSimple()
        {
            var fake = GhostHelper.CreateFake(new UserService());
            this.AddUser(fake, 1);

            GhostHelper.Mode = FakerModeEnum.Load;

            try
            {
                fake.List();
                Assert.IsFalse(true);
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }


        [TestMethod]
        public void DisabledWait()
        {
            var original = new Clock();
            var fake = GhostHelper.CreateFake(original);

            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 1; i++)
            {
                fake.GetTime();
            }
            watch.Stop();
            var originalElapsed = watch.ElapsedMilliseconds;

            watch.Restart();
            GhostHelper.Mode = FakerModeEnum.Load;

            for (int i = 0; i < 1; i++)
            {
                fake.GetTime();
            }
            watch.Stop();
            var fakeElapsed = watch.ElapsedMilliseconds;

            Assert.IsTrue(originalElapsed > fakeElapsed);
        }

        public class UserService
        {
            private List<string> names = new List<string>(){"joe"};

            public virtual List<string> List()
            {
                return names;
            }

            public virtual void Add(string name)
            {
                names.Add(name);
            }

            public virtual void Remove(string name)
            {
                names.Remove(name);
            }
        }

        public class Clock
        {
            public virtual DateTime GetTime()
            {
                Thread.Sleep(500);
                return DateTime.Now;
            }
        }
    }
}