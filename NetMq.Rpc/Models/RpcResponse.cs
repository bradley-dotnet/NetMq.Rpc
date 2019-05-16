using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    internal class RpcResponse
    {
        public object ReturnValue { get; set; }
        public Guid SynchronizationId { get; set; }
    }
}
