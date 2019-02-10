using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    public class PendingMessage
    {
        public byte[] ClientAddress { get; set; }
        public IEnumerable<byte[]> Body { get; set; }
    }
}
