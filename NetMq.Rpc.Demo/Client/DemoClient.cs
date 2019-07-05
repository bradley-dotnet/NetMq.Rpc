using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc.Demo.Client
{
    public class DemoClient : RpcClient<IDemoContract>, IDemoContract
    {
        private readonly ILogger logger;

        public DemoClient(string endpoint, ILogger logger)
            : base (endpoint, logger)
        {
            this.logger = logger;
        }

        public async Task<Box<int>> AddAsync(List<int> addends)
        {
            logger.LogInformation("Starting add");
            var sum = await GetReturnValueAsync<Box<int>>(MakeParams(addends));
            logger.LogInformation($"Got an answer of {sum.Value}");
            return sum;
        }

        public async Task<Box<int>> AddObjectAsync(Addends arguments)
        {
            logger.LogInformation("Starting object add");
            var sum = await GetReturnValueAsync<Box<int>>(MakeParams(arguments));
            logger.LogInformation($"Got an answer of {sum.Value}");
            return sum;
        }
    }
}
