using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Services
{
    internal class Timer : ITimer
    {
        private System.Timers.Timer internalTimer;

        internal Timer(TimeSpan interval, Action callback)
        {
            internalTimer = new System.Timers.Timer();
            internalTimer.Interval = interval.TotalMilliseconds;
            internalTimer.Elapsed += (o, e) => callback();
        }

        public void Start()
        {
            internalTimer.Start();
        }

        public void Stop()
        {
            internalTimer.Stop();
        }

        public void Reset()
        {
            internalTimer.Stop();
            internalTimer.Start();
        }
    }
}
