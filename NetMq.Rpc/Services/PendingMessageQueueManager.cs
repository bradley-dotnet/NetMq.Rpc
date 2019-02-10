using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Services
{
    public class PendingMessageQueueManager : IPendingMessageQueues
    {
        public void Add(string service, byte[] clientAddress, IEnumerable<byte[]> message)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PendingMessage> Get(string service)
        {
            throw new NotImplementedException();
        }

        public void Remove(string service)
        {
            throw new NotImplementedException();
        }
    }
}
