using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Tests.Mocks
{
    class FakeTimer : ITimer
    {
        private Action callback;

        public FakeTimer(Action callback)
        {
            this.callback = callback;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Tick()
        {
            callback();
        }
    }
}
