using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    internal class RpcMessage
    {
        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
        public Guid SynchronizationId { get; set; }
    }
}
