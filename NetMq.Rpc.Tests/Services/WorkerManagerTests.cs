using NetMq.Rpc.Contracts;
using NetMq.Rpc.Services;
using NetMq.Rpc.Tests.Mocks;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Tests.Services
{
    [TestFixture]
    public class WorkerManagerTests
    {
        private byte[] firstWorkerAddress = new byte[1] { 0x01 };
        private byte[] secondWorkerAddress = new byte[1] { 0x02 };
        private string service = "ServiceName";
        private DateTime currentTime;

        private IDateTimeProvider dateTime;
        private ITimerFactory timerFactory;
        private IWorkerManager systemUnderTest;
        private FakeTimer timerFake;

        [SetUp]
        public void Setup()
        {
            dateTime = Substitute.For<IDateTimeProvider>();
            timerFactory = Substitute.For<ITimerFactory>();

            currentTime = DateTime.Now;
            dateTime.Now.Returns(currentTime);
            timerFactory.Create(TimeSpan.Zero, null).ReturnsForAnyArgs(ci => timerFake = new FakeTimer(ci.Arg<Action>()));

            systemUnderTest = new WorkerManager(dateTime, timerFactory);
        }

        [Test]
        public void OnStart_StartsDeadWorkerTimer()
        {
            Assert.IsTrue(timerFake.Started);
        }

        [Test]
        public void OnHeartbeat_WhenWorkerNotPresent_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => systemUnderTest.WorkerHeartbeat(firstWorkerAddress));
        }

        [Test]
        public void OnFetch_WhenNoWorker_ReturnsNull()
        {
            var worker = systemUnderTest.GetWorkerForService(service);
            Assert.IsNull(worker);
        }

        [Test]
        public void OnFetch_WhenSingleWorker_ReturnsWorker()
        {
            systemUnderTest.AddWorker(service, firstWorkerAddress);
            var worker = systemUnderTest.GetWorkerForService(service);
            Assert.AreEqual(firstWorkerAddress, worker);
        }

        [Test]
        public void OnFetch_WhenSeveralWorkers_ReturnsRoundRobin()
        {
            systemUnderTest.AddWorker(service, firstWorkerAddress);
            systemUnderTest.AddWorker(service, secondWorkerAddress);

            var worker = systemUnderTest.GetWorkerForService(service);
            Assert.AreEqual(firstWorkerAddress, worker);
            currentTime += TimeSpan.FromSeconds(1);
            dateTime.Now.Returns(currentTime);

            worker = systemUnderTest.GetWorkerForService(service);
            Assert.AreEqual(secondWorkerAddress, worker);
        }

        [Test]
        public void OnFetch_AfterDisconnect_ReturnsOtherWorker()
        {
            systemUnderTest.AddWorker(service, firstWorkerAddress);
            systemUnderTest.AddWorker(service, secondWorkerAddress);

            var worker = systemUnderTest.GetWorkerForService(service);
            Assert.AreEqual(firstWorkerAddress, worker);
            systemUnderTest.DisconnectWorker(secondWorkerAddress);
            currentTime += TimeSpan.FromSeconds(1);
            dateTime.Now.Returns(currentTime);

            worker = systemUnderTest.GetWorkerForService(service);
            Assert.AreEqual(firstWorkerAddress, worker);
        }

        [Test]
        public void OnFetch_AfterTimeout_ReturnsOtherWorker()
        {
            systemUnderTest.AddWorker(service, firstWorkerAddress);
            systemUnderTest.AddWorker(service, secondWorkerAddress);

            var worker = systemUnderTest.GetWorkerForService(service);
            Assert.AreEqual(firstWorkerAddress, worker);

            currentTime = currentTime + TimeSpan.FromMinutes(5);
            dateTime.Now.Returns(currentTime);
            systemUnderTest.WorkerHeartbeat(firstWorkerAddress);
            timerFake.Tick();

            worker = systemUnderTest.GetWorkerForService(service);
            Assert.AreEqual(firstWorkerAddress, worker);
        }

        [Test]
        public void OnGetAll_ReturnsAllActiveWorkers()
        {
            systemUnderTest.AddWorker(service, firstWorkerAddress);
            systemUnderTest.AddWorker(service, secondWorkerAddress);

            var allWorkers = systemUnderTest.GetWorkerAddresses();
            CollectionAssert.AreEqual(allWorkers, new[] { firstWorkerAddress, secondWorkerAddress });
        }

        [Test]
        public void OnGetServiceForWorker_ReturnsCorrectService()
        {
            systemUnderTest.AddWorker(service, firstWorkerAddress);

            var serviceName = systemUnderTest.GetWorkerService(firstWorkerAddress);
            Assert.AreEqual(service, serviceName);
        }
    }
}
