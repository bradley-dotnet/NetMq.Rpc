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
            return Task.Run(() =>
            {
                method.Invoke(target, parameters);
                return (object)null;
            });
        }

        private Task<object> InvokeSynchronous(object target, MethodInfo method, object[] parameters)
        {
            return Task.Run(() =>
            {
                return method.Invoke(target, parameters);
            });
        }

        private async Task<object> InvokeVoidTask(object target, MethodInfo method, object[] parameters)
        {
            var task = (Task)method.Invoke(target, parameters);
            await task;
            return null;
        }

        private async Task<object> InvokeGenericTask(object target, MethodInfo method, object[] parameters)
        {
            var task = (Task)method.Invoke(target, parameters);
            await task;

            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }
    }
}
