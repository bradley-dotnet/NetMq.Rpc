using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface ITimer
    {
        void Start();
        void Stop();
        void Reset();
    }
}
