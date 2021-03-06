﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    public interface ITimerFactory
    {
        ITimer Create(TimeSpan interval, Action callback);
    }
}
