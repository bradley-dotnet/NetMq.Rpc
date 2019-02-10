using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Services
{
    class MdpClientMessageFactory : IMdpClientMessageFactory
    {
        private bool explicitEnvelope;
        public MdpClientMessageFactory(bool explicitEnvelope)
        {
            this.explicitEnvelope = explicitEnvelope;
        }

        public IEnumerable<byte[]> GenerateRequest(string service, IEnumerable<byte[]> message)
        {
            var frames = new List<byte[]>
            {
                NetMqHelper.ConvertStringToBytes(MdpProtocolNames.Client),
                NetMqHelper.ConvertStringToBytes(service)
            };
            frames.AddRange(message);
            if (explicitEnvelope)
            {
                frames.Insert(0, NetMqHelper.EmptyFrame);
            }
            return frames;
        }
    }
}
