using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface IWorkerManager
    {
        void AddWorker(string service, byte[] workerAddress);
        void WorkerHeartbeat(byte[] workerAddress);
        void DisconnectWorker(byte[] workerAddress);

        string GetWorkerService(byte[] workerAddress);
        byte[] GetWorkerForService(string service);
        IEnumerable<byte[]> GetWorkerAddresses();
    }
}
