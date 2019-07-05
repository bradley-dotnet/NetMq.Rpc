using NetMq.Rpc.Contracts;
using Newtonsoft.Json.Linq;
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
        private Dictionary<string, ParameterInfo[]> parameterCache;

        public MethodCache(Type interfaceType)
        {
            cache = interfaceType.GetMethods().ToDictionary(m => m.Name);
            parameterCache = interfaceType.GetMethods().ToDictionary(m => m.Name, m => m.GetParameters());
        }

        public MethodInfo GetMethod(string methodName)
        {
            return cache[methodName];
        }

        public object[] SanitizeParameters(string methodName, object[] sourceParameters)
        {
            var parameterInfo = parameterCache[methodName];
            var sanitizedParameters = new object[sourceParameters.Length];
            for (var i = 0; i < sourceParameters.Length; i++)
            {
                sanitizedParameters[i] = SanitizeParameter(sourceParameters[i], parameterInfo[i]);
            }
            return sanitizedParameters;
        }

        private object SanitizeParameter(object source, ParameterInfo target)
        {
            if (source is JToken json)
            {
                return json.ToObject(target.ParameterType);
            }
            return source;
        }
    }
}
