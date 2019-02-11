using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using NetMq.Rpc.Services;
using NetMq.Rpc.Tests.Mocks;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Tests.Services
{
    [TestFixture]
    public class PendingMessageQueueManagerTests
    {
        private DateTime currentTime;

        private IDateTimeProvider dateTime;
        private ITimerFactory timerFactory;
        private FakeTimer timerFake;
        private PendingMessageQueueManager systemUnderTest;

        private byte[] clientAddress = new byte[] { 0x01 };
        private byte[] message = new byte[] { 0x02 };
        private string service = "ServiceName";
        private string otherService = "OtherService";

        [SetUp]
        public void Setup()
        {
            dateTime = Substitute.For<IDateTimeProvider>();
            timerFactory = Substitute.For<ITimerFactory>();

            currentTime = DateTime.Now;
            dateTime.Now.Returns(currentTime);
            timerFactory.Create(TimeSpan.Zero, null).ReturnsForAnyArgs(ci => timerFake = new FakeTimer(ci.Arg<Action>()));

            systemUnderTest = new PendingMessageQueueManager(dateTime, timerFactory);
        }

        [Test]
        public void OnStart_StartsDeadMessageTimer()
        {
            Assert.IsTrue(timerFake.Started);
        }

        [Test]
        public void OnGet_WhenMessagesQueued_ReturnsAllMessages()
        {
            systemUnderTest.Add(service, clientAddress, new List<byte[]> { message });
            systemUnderTest.Add(service, clientAddress, new List<byte[]> { message });
            systemUnderTest.Add(otherService, clientAddress, new List<byte[]> { message });

            var queue = systemUnderTest.Get(service);
            Assert.AreEqual(2, queue.Count());
        }

        [Test]
        public void OnGet_WhenNoQueue_ReturnsEmptyEnumerable()
        {
            systemUnderTest.Add(service, clientAddress, new List<byte[]> { message });
            systemUnderTest.Add(service, clientAddress, new List<byte[]> { message });

            var queue = systemUnderTest.Get(otherService);
            Assert.AreEqual(0, queue.Count());
        }

        [Test]
        public void OnRemove_WhenNoQueue_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => systemUnderTest.Remove(service));
        }

        [Test]
        public void OnGet_WhenDeadMessages_ReturnsOnlyLiveMessages()
        {
            systemUnderTest.Add(service, clientAddress, new List<byte[]> { message });
            systemUnderTest.Add(service, clientAddress, new List<byte[]> { message });

            currentTime += TimeSpan.FromMinutes(5);
            dateTime.Now.Returns(currentTime);

            systemUnderTest.Add(service, clientAddress, new List<byte[]> { message });
            timerFake.Tick();

            var queue = systemUnderTest.Get(service);
            Assert.AreEqual(1, queue.Count());
        }
    }
}
