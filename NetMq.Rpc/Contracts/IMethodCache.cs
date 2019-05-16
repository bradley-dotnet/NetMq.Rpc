using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface IMethodCache
    {
        MethodInfo GetMethod(string methodName);
        object[] SanitizeParameters(string methodName, object[] sourceParameters);
    }
}
