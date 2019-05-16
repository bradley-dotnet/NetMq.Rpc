using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc.Demo.Worker
{
    public class DemoWorker : RpcWorker<IDemoContract>, IDemoContract
    {
        private readonly ILogger logger;
        public DemoWorker(string endpoint, ILogger logger)
            : base(endpoint, logger)
        {
            this.logger = logger;
        }

        public async Task<Box<int>> AddAsync(List<int> addends)
        {
            logger.LogInformation("Adding with delay");

            var sum = addends.Sum();
            await Task.Delay(1000);
            return new Box<int> { Value = sum };
        }
    }
}
