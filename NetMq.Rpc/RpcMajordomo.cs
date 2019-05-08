using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using NetMq.Rpc.Services;
using NetMq.Rpc.Sockets;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("NetMq.Rpc.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace NetMq.Rpc
{
    public sealed class RpcMajordomo : IRpcMajordomo
    {
        private readonly IWorkerManager workerManager;
        private readonly IPendingMessageQueues pendingMessageQueues;
        private readonly ISocket socket;
        private readonly IMdpBrokerMessageFactory messageFactory;

        public RpcMajordomo(string endpoint)
            : this(new WorkerManager(new SystemDateTimeProvider(), new TimerFactory()), 
                  new PendingMessageQueueManager(new SystemDateTimeProvider(), new TimerFactory()), 
                  new MdpBrokerMessageFactory(), 
                  new RouterSocket(endpoint))
        {
        }

        public RpcMajordomo(IWorkerManager workerManager,
            IPendingMessageQueues pendingMessageQueues,
            IMdpBrokerMessageFactory messageFactory,
            ISocket socket)
        {
            this.workerManager = workerManager;
            this.pendingMessageQueues = pendingMessageQueues;
            this.messageFactory = messageFactory;
            this.socket = socket;

            socket.MessageReady += ParseMessage;
            socket.AddTimer(TimeSpan.FromSeconds(1), GenerateHeartbeats);
        }

        public void Dispose()
        {
            socket.Dispose();
        }

        private void ParseMessage()
        {
            var message = socket.GetNextMessage();
            var clientAddress = message.ElementAt(0).ToByteArray();
            var protocol = message.ElementAt(2).ConvertToString();

            var remainingFrames = message.Skip(3);
            switch (protocol)
            {
                case MdpProtocolNames.Client:
                    ParseClientMessage(clientAddress, remainingFrames);
                    break;
                case MdpProtocolNames.Worker:
                    ParseWorkerMessage(clientAddress, remainingFrames);
                    break;
            }
        }

        private void ParseClientMessage(byte[] clientAddress, IEnumerable<NetMQFrame> frames)
        {
            // This is always a REQUEST
            var service = frames.First().ConvertToString();
            var message = frames.Skip(1).Select(f => f.ToByteArray());

            var worker = workerManager.GetWorkerForService(service);

            if (worker == null)
            {
                pendingMessageQueues.Add(service, clientAddress, message);
            }
            else
            {
                socket.SendMessage(messageFactory.GenerateWorkerRequest(worker, clientAddress, message));
            }
        }

        private void ParseWorkerMessage(byte[] clientAddress, IEnumerable<NetMQFrame> frames)
        {
            //All messages are heartbeats
            workerManager.WorkerHeartbeat(clientAddress);

            var command = (MdpWorkerProtocol)frames.First().Buffer[0];
            var remainingFrames = frames.Skip(1);
            switch (command)
            {
                case MdpWorkerProtocol.Ready:
                    HandleWorkerReady(clientAddress, remainingFrames);
                    break;
                case MdpWorkerProtocol.Reply:
                    HandleWorkerReply(clientAddress, remainingFrames);
                    break;
                case MdpWorkerProtocol.Heartbeat:
                    // Need to make sure the worker has sent "ready". The method sends disconnect if it hasn't
                    CheckAndGetServiceForAddress(clientAddress);
                    break;
                case MdpWorkerProtocol.Disconnect:
                    workerManager.DisconnectWorker(clientAddress);
                    break;
            }
        }

        private void GenerateHeartbeats()
        {
            foreach (var worker in workerManager.GetWorkerAddresses())
            {
                socket.SendMessage(messageFactory.GenerateHeartbeat(worker));
            }
        }

        private void HandleWorkerReady(byte[] address, IEnumerable<NetMQFrame> frames)
        {
            var service = frames.First().ConvertToString();
            workerManager.AddWorker(service, address);

            foreach (var pendingMessage in pendingMessageQueues.Get(service))
            {
                socket.SendMessage(messageFactory.GenerateWorkerRequest(address, pendingMessage.ClientAddress, pendingMessage.Body));
            }
            pendingMessageQueues.Remove(service);
        }

        private void HandleWorkerReply(byte[] address, IEnumerable<NetMQFrame> frames)
        {
            var requestingClient = frames.ElementAt(0).ToByteArray();
            var reply = frames.Skip(2).Select(f => f.ToByteArray());

            var service = CheckAndGetServiceForAddress(address);
            if (!string.IsNullOrEmpty(service))
            {
                socket.SendMessage(messageFactory.GenerateClientReply(requestingClient, service, reply));
            }
        }

        private string CheckAndGetServiceForAddress(byte[] address)
        {
            var service = workerManager.GetWorkerService(address);
            if (string.IsNullOrEmpty(service))
            {
                socket.SendMessage(messageFactory.GenerateDisconnect(address));
            }
            return service;
        }
    }
}
