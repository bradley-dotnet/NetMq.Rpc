using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    interface ITimerFactory
    {
        ITimer Create(TimeSpan interval, Action callback);
    }
}
