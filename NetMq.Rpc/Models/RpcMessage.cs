﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    internal class RpcMessage
    {
        internal string MethodName { get; set; }
        internal object[] Parameters { get; set; }
        internal Guid SynchronizationId { get; set; }
    }
}
