using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Services
{
    class MdpWorkerMessageFactory : IMdpWorkerMessageFactory
    {
        public IEnumerable<byte[]> GenerateReady(string service)
        {
            return new List<byte[]>
            {
                NetMqHelper.EmptyFrame,
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Ready },
                NetMqHelper.ConvertStringToBytes(service),
            };
        }

        public IEnumerable<byte[]> GenerateReply(byte[] clientAddress, IEnumerable<byte[]> body)
        {
            return new List<byte[]>
            {
                NetMqHelper.EmptyFrame,
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Reply },
                clientAddress,
                NetMqHelper.EmptyFrame
            }.Concat(body);
        }

        public IEnumerable<byte[]> GenerateDisconnect()
        {
            return new List<byte[]>
            {
                NetMqHelper.EmptyFrame,
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Disconnect }
            };
        }

        public IEnumerable<byte[]> GenerateHeartbeat()
        {
            return new List<byte[]>
            {
                NetMqHelper.EmptyFrame,
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Worker),
                new byte[] { (byte)MdpWorkerProtocol.Heartbeat }
            };
        }
    }
}
