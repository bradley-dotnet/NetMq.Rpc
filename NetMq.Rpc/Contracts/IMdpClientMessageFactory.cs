using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface IMdpClientMessageFactory
    {
        IEnumerable<byte[]> GenerateRequest(string service, IEnumerable<byte[]> message);
    }
}
