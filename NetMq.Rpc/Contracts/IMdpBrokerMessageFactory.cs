using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface IMdpBrokerMessageFactory
    {
        IEnumerable<byte[]> GenerateHeartbeat(byte[] workerAddress);
        IEnumerable<byte[]> GenerateWorkerRequest(byte[] workerAddress, byte[] clientAddress, IEnumerable<byte[]> message);
        IEnumerable<byte[]> GenerateClientReply(byte[] destination, string service, IEnumerable<byte[]> message);
        IEnumerable<byte[]> GenerateDisconnect(byte[] workerAddress);
    }
}
