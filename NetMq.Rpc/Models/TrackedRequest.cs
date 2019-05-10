using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc.Models
{
    public class TrackedRequest<T> : ITrackedRequest
    {
        private TaskCompletionSource<T> taskSource;

        public Task<T> Task => taskSource.Task;
        public DateTime RequestTime { get; }

        public TrackedRequest(DateTime now)
        {
            taskSource = new TaskCompletionSource<T>();
            RequestTime = now;
        }

        public void SetResult(object result)
        {
            taskSource.TrySetResult((T)result);
        }

        public void Cancel()
        {
            taskSource.TrySetException(new TimeoutException("Message response not recieved in 30 seconds"));
        }
    }
}
