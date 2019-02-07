using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc
{
    public abstract class RpcWorker<TContract> : IRpcWorker<TContract>
        where TContract : class
    {
        public RpcWorker()
        {
            if (!GetType().IsAssignableFrom(typeof(TContract)))
            {
                throw new InvalidOperationException("Worker must implement contract interface");
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
