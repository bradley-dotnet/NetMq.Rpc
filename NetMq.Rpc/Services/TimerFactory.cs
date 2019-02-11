using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Services
{
    class TimerFactory : ITimerFactory
    {
        public ITimer Create(TimeSpan interval, Action callback)
        {
            return new Timer(interval, callback);
        }
    }
}
