using NetMq.Rpc.Contracts;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Sockets
{
    public abstract class BaseSocket : ISocket
    {
        public event Action MessageReady;
        private readonly NetMQSocket socket;
        private NetMQPoller poller;

        public BaseSocket(NetMQSocket concreteSocket)
        {
            socket = concreteSocket;
            socket.ReceiveReady += (s, e) => MessageReady?.Invoke();

            poller = new NetMQPoller { socket };
            poller.RunAsync();
        }

        public void AddTimer(TimeSpan interval, Action callback)
        {
            NetMQTimer timer = new NetMQTimer(interval);
            timer.Elapsed += (s, e) => callback();

            poller.Add(timer);
        }

        public void Dispose()
        {
            poller.Stop();

            poller.Dispose();
            socket.Dispose();
        }

        public NetMQMessage GetNextMessage()
        {
            return socket.ReceiveMultipartMessage();
        }

        public void SendMessage(IEnumerable<byte[]> messageFrames)
        {
            socket.SendMultipartBytes(messageFrames.ToArray());
        }
    }
}
