using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Models
{
    interface ITrackedRequest
    {
        DateTime RequestTime { get; }

        void SetResult(object result);

        void Cancel();
    }
}
