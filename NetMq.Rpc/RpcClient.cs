using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc
{
    public class RpcClient<TContract> : IRpcClient<TContract>
        where TContract : class
    {
        public RpcClient()
        {
            if (!GetType().IsAssignableFrom(typeof(TContract)))
            {
                throw new InvalidOperationException("Client must implement contract interface");
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
