using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Services
{
    internal class MdpBrokerMessageFactory : IMdpBrokerMessageFactory
    {
        public IEnumerable<byte[]> GenerateHeartbeat(byte[] workerAddress)
        {
            return new List<byte[]>
            {
                workerAddress,
                NetMqHelper.EmptyFrame,
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Heartbeat }
            };
        }

        public IEnumerable<byte[]> GenerateWorkerRequest(byte[] workerAddress, byte[] clientAddress, IEnumerable<byte[]> message)
        {
            return new List<byte[]>
            {
                workerAddress,
                NetMqHelper.EmptyFrame,
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Request },
                clientAddress,
                NetMqHelper.EmptyFrame,
            }.Concat(message);
        }

        public IEnumerable<byte[]> GenerateClientReply(byte[] destination, string service, IEnumerable<byte[]> message)
        {
            return new List<byte[]>
            {
                destination,
                NetMqHelper.EmptyFrame,
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Client),
                NetMqHelper.ConvertStringToBytes(service),
            }.Concat(message);
        }

        public IEnumerable<byte[]> GenerateDisconnect(byte[] workerAddress)
        {
            return new List<byte[]>
            {
                workerAddress,
                NetMqHelper.EmptyFrame,
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Disconnect }
            };
        }
    }
}
