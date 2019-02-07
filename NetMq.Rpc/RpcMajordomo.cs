using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleToAttribute("NetMq.Rpc.Tests")]
namespace NetMq.Rpc
{
    public sealed class RpcMajordomo : IRpcMajordomo
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
