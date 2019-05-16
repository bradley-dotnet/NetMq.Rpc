using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using NetMq.Rpc.Services;
using NetMQ;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Tests
{
    [TestFixture]
    public class RpcMajordomoTests
    {
        private IWorkerManager workerManager;
        private IPendingMessageQueues pendingMessageQueues;
        private ISocket socket;
        private IMdpBrokerMessageFactory messageFactory;
        private IMdpWorkerMessageFactory workerMessageFactory;
        private IMdpClientMessageFactory clientMessageFactory;

        private RpcMajordomo systemUnderTest;
        private string service = "ServiceName";
        private byte[] clientAddress = new byte[1] { 0x0F };
        private byte[] workerAddress = new byte[1] { 0x01 };
        private IEnumerable<byte[]> message = new List<byte[]> { new byte[1] { 0x02} };

        [SetUp]
        public void Setup()
        {
            workerManager = Substitute.For<IWorkerManager>();
            pendingMessageQueues = Substitute.For<IPendingMessageQueues>();
            socket = Substitute.For<ISocket>();
            messageFactory = Substitute.For<IMdpBrokerMessageFactory>();

            workerMessageFactory = new MdpWorkerMessageFactory();
            clientMessageFactory = new MdpClientMessageFactory(true);
            systemUnderTest = new RpcMajordomo(workerManager, pendingMessageQueues, messageFactory, socket, null);
        }

        [Test]
        public void OnClientRequest_WhenWorkerAvailable_BrokerForwardsRequest()
        {
            workerManager.GetWorkerForService(service).Returns(workerAddress);
            workerManager.GetWorkerService(workerAddress).Returns(service);

            socket.GetNextMessage().Returns(GenerateMessageFromClient(clientMessageFactory.GenerateRequest(service, message)));
            socket.MessageReady += Raise.Event<Action>();

            messageFactory.Received().GenerateWorkerRequest(workerAddress, clientAddress, Arg.Is<IEnumerable<byte[]>>(m => m.SequenceEqual(message)));
        }

        [Test]
        public void OnClientRequest_WhenWorkerNotAvailable_BrokerQueuesRequest()
        {
            workerManager.GetWorkerForService(service).Returns((byte[])null);

            socket.GetNextMessage().Returns(GenerateMessageFromClient(clientMessageFactory.GenerateRequest(service, message)));
            socket.MessageReady += Raise.Event<Action>();

            messageFactory.DidNotReceiveWithAnyArgs().GenerateWorkerRequest(workerAddress, clientAddress, message);
            pendingMessageQueues.Received().Add(service, clientAddress, Arg.Is<IEnumerable<byte[]>>(m => m.SequenceEqual(message)));
        }

        [Test]
        public void OnStarting_BrokerSetsUpHeartbeatForAllWorkers()
        {
            workerManager.GetWorkerAddresses().Returns(new List<byte[]> { workerAddress, workerAddress });
            socket.Received().AddTimer(Arg.Any<TimeSpan>(), Arg.Is<Action>(c => CheckTimerCallback(c)));
        }

        private bool CheckTimerCallback(Action callback)
        {
            callback();
            messageFactory.Received(2).GenerateHeartbeat(workerAddress);
            return true;
        }

        [Test]
        public void OnWorkerReady_WithoutPendingMessages_BrokerOnlyAddsWorkerToPool()
        {
            socket.GetNextMessage().Returns(GenerateMessageFromWorker(workerMessageFactory.GenerateReady(service)));
            socket.MessageReady += Raise.Event<Action>();

            workerManager.Received().AddWorker(service, workerAddress);
            messageFactory.DidNotReceiveWithAnyArgs().GenerateWorkerRequest(null, null, null);
        }

        [Test]
        public void OnWorkerReadyWithPendingMessages_BrokerAddsWorkerToPoolAndSendsQueue()
        {
            pendingMessageQueues.Get(service).Returns(new List<PendingMessage>
            {
                new PendingMessage(clientAddress, message, DateTime.Now),
                new PendingMessage(clientAddress, message, DateTime.Now)
            });
            socket.GetNextMessage().Returns(GenerateMessageFromWorker(workerMessageFactory.GenerateReady(service)));
            socket.MessageReady += Raise.Event<Action>();

            workerManager.Received().AddWorker(service, workerAddress);
            messageFactory.Received(2).GenerateWorkerRequest(workerAddress, clientAddress, message);
        }

        [Test]
        public void OnWorkerReply_WhenWorkerReady_BrokerSendsBackToRequestingClient()
        {
            workerManager.GetWorkerForService(service).Returns(workerAddress);
            workerManager.GetWorkerService(workerAddress).Returns(service);

            socket.GetNextMessage().Returns(GenerateMessageFromWorker(workerMessageFactory.GenerateReply(clientAddress, message)));
            socket.MessageReady += Raise.Event<Action>();

            messageFactory.Received().GenerateClientReply(clientAddress, service, Arg.Is<IEnumerable<byte[]>>(m => m.SequenceEqual(message)));
        }

        [Test]
        public void OnWorkerReply_WhenWorkerNotReady_BrokerSendsDisconnect()
        {
            socket.GetNextMessage().Returns(GenerateMessageFromWorker(workerMessageFactory.GenerateReply(clientAddress, message)));
            socket.MessageReady += Raise.Event<Action>();

            messageFactory.Received().GenerateDisconnect(workerAddress);
            messageFactory.DidNotReceiveWithAnyArgs().GenerateClientReply(clientAddress, service, message);
        }

        [Test]
        public void OnWorkerHeartbeat_WhenWorkerNotReady_BrokerSendsDisconnect()
        {
            socket.GetNextMessage().Returns(GenerateMessageFromWorker(workerMessageFactory.GenerateHeartbeat()));
            socket.MessageReady += Raise.Event<Action>();

            messageFactory.Received().GenerateDisconnect(workerAddress);
        }

        [Test]
        public void OnWorkerHeartbeat_WhenWorkerReady_BrokerNotifiesWorkerManager()
        {
            workerManager.GetWorkerService(workerAddress).Returns(service);

            socket.GetNextMessage().Returns(GenerateMessageFromWorker(workerMessageFactory.GenerateHeartbeat()));
            socket.MessageReady += Raise.Event<Action>();

            workerManager.Received().WorkerHeartbeat(workerAddress);
            socket.DidNotReceiveWithAnyArgs().SendMessage(null);
        }

        [Test]
        public void OnWorkerDisconnect_BrokerNotifiesWorkerManager()
        {
            socket.GetNextMessage().Returns(GenerateMessageFromWorker(workerMessageFactory.GenerateDisconnect()));
            socket.MessageReady += Raise.Event<Action>();

            workerManager.Received().DisconnectWorker(workerAddress);
        }

        private NetMQMessage GenerateMessageFromClient(IEnumerable<byte[]> body)
        {
            var message = new List<byte[]>() { clientAddress };
            message.AddRange(body);
            return new NetMQMessage(message);
        }

        private NetMQMessage GenerateMessageFromWorker(IEnumerable<byte[]> body)
        {
            var message = new List<byte[]>() { workerAddress };
            message.AddRange(body);
            return new NetMQMessage(message);
        }
    }
}
