using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    interface ITimer
    {
        void Start();
        void Stop();
    }
}
