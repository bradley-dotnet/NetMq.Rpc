using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc.Services
{
    internal class WorkerManager : IWorkerManager
    {
        private readonly IDateTimeProvider dateTime;
        private readonly ITimer heartbeatChecker;

        private readonly Dictionary<string, List<MdpWorker>> serviceWorkers = new Dictionary<string, List<MdpWorker>>();
        private readonly Dictionary<byte[], MdpWorker> workers = new Dictionary<byte[], MdpWorker>(new ByteArrayComparer());
        private readonly TimeSpan deadWorkerTimeout = TimeSpan.FromSeconds(30);

        public WorkerManager(IDateTimeProvider dateTime,
            ITimerFactory timerFactory)
        {
            this.dateTime = dateTime;
            heartbeatChecker = timerFactory.Create(TimeSpan.FromSeconds(30), CheckForDeadWorkers);
        }

        public void AddWorker(string service, byte[] workerAddress)
        {
            var worker = new MdpWorker(service, workerAddress, dateTime.Now);
            if (!serviceWorkers.TryGetValue(service, out var workerList))
            {
                workerList = new List<MdpWorker>();
                serviceWorkers.Add(service, workerList);
            }
            workerList.Add(worker);
            workers.Add(workerAddress, worker);
        }

        public void DisconnectWorker(byte[] workerAddress)
        {
            if (workers.TryGetValue(workerAddress, out var worker))
            {
                var service = worker.Service;
                if (serviceWorkers.TryGetValue(service, out var workerList))
                {
                    workerList.Remove(worker);
                    if (!workerList.Any())
                    {
                        serviceWorkers.Remove(service);
                    }
                }
                workers.Remove(workerAddress);
            }
        }

        public IEnumerable<byte[]> GetWorkerAddresses()
        {
            return workers.Keys;
        }

        public byte[] GetWorkerForService(string service)
        {
            if (serviceWorkers.TryGetValue(service, out var workerList))
            {
                var selectedWorker = workerList.OrderBy(w => w.LastUsage).First();
                selectedWorker.LastUsage = dateTime.Now;
                return selectedWorker.Address;
            }
            else
            {
                return null;
            }
        }

        public string GetWorkerService(byte[] workerAddress)
        {
            if (workers.TryGetValue(workerAddress, out var worker))
            {
                return worker.Service;
            }
            else
            {
                return null;
            }
        }

        public void WorkerHeartbeat(byte[] workerAddress)
        {
            if (workers.TryGetValue(workerAddress, out var worker))
            {
                worker.LastHeartbeat = dateTime.Now;
            }
        }

        private void CheckForDeadWorkers()
        {
            var compareTime = dateTime.Now;
            foreach (var worker in workers.Values.Reverse())
            {
                if (compareTime - worker.LastHeartbeat > deadWorkerTimeout)
                {
                    DisconnectWorker(worker.Address);
                }
            }
        }
    }
}
