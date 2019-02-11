using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Contracts
{
    interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
