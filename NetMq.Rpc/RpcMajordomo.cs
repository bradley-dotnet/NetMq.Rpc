using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleToAttribute("NetMq.Rpc.Tests")]
namespace NetMq.Rpc
{
    public sealed class RpcMajordomo : IRpcMajordomo
    {
        private readonly IWorkerManager workerManager;
        private readonly IPendingMessageQueues pendingMessageQueues;
        private readonly ISocket socket;
        private readonly byte[] EmptyFrame = new byte[0];

        public RpcMajordomo(IWorkerManager workerManager,
            IPendingMessageQueues pendingMessageQueues,
            ISocket socket)
        {
            this.workerManager = workerManager;
            this.pendingMessageQueues = pendingMessageQueues;
            this.socket = socket;

            socket.MessageReady += ParseMessage;
            socket.AddTimer(1000, GenerateHeartbeats);
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
                socket.SendMessage(GenerateWorkerRequest(worker, clientAddress, message));
            }
        }

        private void ParseWorkerMessage(byte[] clientAddress, IEnumerable<NetMQFrame> frames)
        {
            //All messages are heartbeats
            workerManager.WorkerHeartbeat(clientAddress);

            var command = (MdpWorkerProtocol)frames.First().ConvertToInt32();
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
                socket.SendMessage(GenerateHeartbeat(worker));
            }
        }

        private void HandleWorkerReady(byte[] address, IEnumerable<NetMQFrame> frames)
        {
            var service = frames.First().ConvertToString();
            workerManager.AddWorker(service, address);

            foreach (var pendingMessage in pendingMessageQueues.Get(service))
            {
                socket.SendMessage(GenerateWorkerRequest(address, pendingMessage.ClientAddress, pendingMessage.Body));
            }
            pendingMessageQueues.Remove(service);
        }

        private void HandleWorkerReply(byte[] address, IEnumerable<NetMQFrame> frames)
        {
            var requestingClient = frames.ElementAt(0).ToByteArray();
            var reply = frames.Skip(2).Select(f => f.ToByteArray());

            var service = CheckAndGetServiceForAddress(address);

            socket.SendMessage(GenerateClientReply(requestingClient, service, reply));
        }

        private string CheckAndGetServiceForAddress(byte[] address)
        {
            var service = workerManager.GetWorkerService(address);
            if (service == null)
            {
                socket.SendMessage(GenerateDisconnect(address));
            }
            return service;
        }

        private IEnumerable<byte[]> GenerateHeartbeat(byte[] workerAddress)
        {
            return new List<byte[]>
            {
                workerAddress,
                EmptyFrame,
                ConvertStringToByets(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Heartbeat }
            };
        }

        private IEnumerable<byte[]> GenerateWorkerRequest(byte[] workerAddress, byte[] clientAddress, IEnumerable<byte[]> message)
        {
            return new List<byte[]>
            {
                workerAddress,
                EmptyFrame,
                ConvertStringToByets(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Request },
                clientAddress,
                EmptyFrame,
            }.Concat(message);
        }

        private IEnumerable<byte[]> GenerateClientReply(byte[] destination, string service, IEnumerable<byte[]> message)
        {
            return new List<byte[]>
            {
                destination,
                EmptyFrame,
                ConvertStringToByets(MdpProtocolNames.Client),
                ConvertStringToByets(service),
            }.Concat(message);
        }

        private IEnumerable<byte[]> GenerateDisconnect(byte[] workerAddress)
        {
            return new List<byte[]>
            {
                workerAddress,
                EmptyFrame,
                ConvertStringToByets(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Disconnect }
            };
        }

        private byte[] ConvertStringToByets(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }
    }
}
