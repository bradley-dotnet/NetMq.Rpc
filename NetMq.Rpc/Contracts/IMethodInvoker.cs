using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc.Contracts
{
    public interface IMethodInvoker
    {
        Task<object> GetMethodResult(object target, MethodInfo method, object[] parameters);
    }
}
