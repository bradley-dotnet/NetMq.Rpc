using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface ISocketFactory
    {
        ISocket Create();
    }
}
