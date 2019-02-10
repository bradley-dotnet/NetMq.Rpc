using NetMQ;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface ISocket : IDisposable
    {
        event Action MessageReady;

        NetMQMessage GetNextMessage();
        void SendMessage(IEnumerable<byte[]> messageFrames);

        void AddTimer(int interval, Action callback);
    }
}
