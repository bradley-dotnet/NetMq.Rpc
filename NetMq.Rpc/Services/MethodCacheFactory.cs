using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Services
{
    internal class MethodCacheFactory : IMethodCacheFactory
    {
        public IMethodCache ConstructCache(Type interfaceType)
        {
            return new MethodCache(interfaceType);
        }
    }
}
