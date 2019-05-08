using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Sockets
{
    public class SocketFactory<T> : ISocketFactory
        where T : ISocket
    {
        private string endpoint;

        public SocketFactory(string endpoint)
        {
            this.endpoint = endpoint;
        }

        public ISocket Create()
        {
            return (T)Activator.CreateInstance(typeof(T), endpoint);
        }
    }
}
