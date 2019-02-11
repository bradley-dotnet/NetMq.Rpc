using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Services
{
    internal class PendingMessageQueueManager : IPendingMessageQueues
    {
        private readonly IDateTimeProvider dateTime;
        private readonly ITimer deadMessageChecker;
        private readonly TimeSpan deadMessageTimeout = TimeSpan.FromSeconds(30);

        private Dictionary<string, Queue<PendingMessage>> pendingQueues = new Dictionary<string, Queue<PendingMessage>>();

        public PendingMessageQueueManager(IDateTimeProvider dateTime,
            ITimerFactory timerFactory)
        {
            this.dateTime = dateTime;
            deadMessageChecker = timerFactory.Create(TimeSpan.FromSeconds(30), CheckForDeadMessages);
            deadMessageChecker.Start();
        }

        public void Add(string service, byte[] clientAddress, IEnumerable<byte[]> message)
        {
            var messageHolder = new PendingMessage(clientAddress, message, dateTime.Now);
            if (!pendingQueues.TryGetValue(service, out var queue))
            {
                queue = new Queue<PendingMessage>();
                pendingQueues.Add(service, queue);
            }
            queue.Enqueue(messageHolder);
        }

        public IEnumerable<PendingMessage> Get(string service)
        {
            if (pendingQueues.TryGetValue(service, out var queue))
            {
                return queue;
            }
            else
            {
                return Enumerable.Empty<PendingMessage>();
            }
        }

        public void Remove(string service)
        {
            if (pendingQueues.ContainsKey(service))
            {
                pendingQueues.Remove(service);
            }
        }

        private void CheckForDeadMessages()
        {
            var compareTime = dateTime.Now;
            foreach (var queue in pendingQueues.Values)
            {
                while (compareTime - queue.Peek().CreationTime > deadMessageTimeout)
                {
                    queue.Dequeue();
                }
            }
        }
    }
}
