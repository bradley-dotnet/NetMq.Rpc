using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    internal enum MdpWorkerProtocol : byte
    {
        Ready = 0x01,
        Request = 0x02,
        Reply = 0x03,
        Heartbeat = 0x04,
        Disconnect = 0x05
    }
}
