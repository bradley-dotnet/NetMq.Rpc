using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc.Services
{
    internal class MethodInvoker : IMethodInvoker
    {
        public Task<object> GetMethodResult(object target, MethodInfo method, object[] parameters)
        {
            if (method.ReturnType == typeof(void))
            {
                return InvokeVoid(target, method, parameters);
            }
            else if (method.ReturnType == typeof(Task))
            {
                return InvokeVoidTask(target, method, parameters);
            }
            else if (method.ReturnType == typeof(Task<>))
            {
                return InvokeGenericTask(target, method, parameters);
            }
            else
            {
                return InvokeSynchronous(target, method, parameters);
            }
        }

        private Task<object> InvokeVoid(object target, MethodInfo method, object[] parameters)
        {
            return Task.FromResult<object>(null);
        }

        private Task<object> InvokeSynchronous(object target, MethodInfo method, object[] parameters)
        {
            return Task.FromResult<object>(null);
        }

        private Task<object> InvokeVoidTask(object target, MethodInfo method, object[] parameters)
        {
            return Task.FromResult<object>(null);
        }

        private Task<object> InvokeGenericTask(object target, MethodInfo method, object[] parameters)
        {
            return Task.FromResult<object>(null);
        }
    }
}
