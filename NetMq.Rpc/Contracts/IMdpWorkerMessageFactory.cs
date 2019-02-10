using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    interface IMdpWorkerMessageFactory
    {
        IEnumerable<byte[]> GenerateReady(string service);
        IEnumerable<byte[]> GenerateReply(byte[] clientAddress, IEnumerable<byte[]> body);
        IEnumerable<byte[]> GenerateHeartbeat();
        IEnumerable<byte[]> GenerateDisconnect();
    }
}
