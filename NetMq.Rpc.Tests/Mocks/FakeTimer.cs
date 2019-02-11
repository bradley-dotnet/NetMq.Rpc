using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Tests.Mocks
{
    class FakeTimer : ITimer
    {
        private Action callback;
        public bool Started { get; private set; }

        public FakeTimer(Action callback)
        {
            this.callback = callback;
        }

        public void Start()
        {
            Started = true;
        }

        public void Stop()
        {
            Started = false;
        }

        public void Tick()
        {
            callback();
        }
    }
}
