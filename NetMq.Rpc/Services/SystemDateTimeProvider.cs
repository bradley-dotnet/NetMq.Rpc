using NetMq.Rpc.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMq.Rpc.Services
{
    class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
