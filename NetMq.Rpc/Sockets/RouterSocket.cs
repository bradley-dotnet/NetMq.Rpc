using NetMq.Rpc.Contracts;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Sockets
{
    public class RouterSocket : ISocket
    {
        public event Action MessageReady;
        private readonly NetMQ.Sockets.RouterSocket socket;
        private NetMQPoller poller;

        public RouterSocket(string endpoint)
        {
            socket = new NetMQ.Sockets.RouterSocket(endpoint);
            socket.ReceiveReady += (s, e) => GetNextMessage();

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
