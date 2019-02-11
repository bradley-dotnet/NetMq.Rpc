using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    public class PendingMessage
    {
        public PendingMessage(byte[] clientAddress, IEnumerable<byte[]> message, DateTime now)
        {
            ClientAddress = clientAddress;
            Body = message;
            CreationTime = now;
        }

        public byte[] ClientAddress { get; set; }
        public IEnumerable<byte[]> Body { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
