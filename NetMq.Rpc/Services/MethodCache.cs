using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetMq.Rpc.Services
{
    internal class MethodCache : IMethodCache
    {
        private Dictionary<string, MethodInfo> cache;

        public MethodCache(Type interfaceType)
        {
            cache = interfaceType.GetMethods().ToDictionary(m => m.Name);
        }

        public MethodInfo GetMethod(string methodName)
        {
            return cache[methodName];
        }
    }
}
