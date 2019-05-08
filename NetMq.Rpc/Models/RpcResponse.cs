using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    public class RpcResponse
    {
        internal object ReturnValue { get; set; }
        internal Guid SynchronizationId { get; set; }
    }
}
