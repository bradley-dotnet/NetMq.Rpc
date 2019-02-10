using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Services
{
    internal class WorkerManager : IWorkerManager
    {
        private Dictionary<decimal, List<MdpWorker>> workers;

        public void AddWorker(string service, byte[] workerAddress)
        {
            throw new NotImplementedException();
        }

        public void DisconnectWorker(byte[] workerAddress)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<byte[]> GetWorkerAddresses()
        {
            throw new NotImplementedException();
        }

        public byte[] GetWorkerForService(string service)
        {
            throw new NotImplementedException();
        }

        public string GetWorkerService(byte[] workerAddress)
        {
            throw new NotImplementedException();
        }

        public void WorkerHeartbeat(byte[] workerAddress)
        {
            throw new NotImplementedException();
        }
    }
}
