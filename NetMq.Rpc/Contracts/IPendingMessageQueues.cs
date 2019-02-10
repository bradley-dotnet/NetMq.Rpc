using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface IPendingMessageQueues
    {
        void Add(string service, byte[] clientAddress, IEnumerable<byte[]> message);
        IEnumerable<PendingMessage> Get(string service);
        void Remove(string service);
    }
}
