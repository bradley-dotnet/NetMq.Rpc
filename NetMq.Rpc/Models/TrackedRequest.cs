using Newtonsoft.Json.Linq;
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
            T converted;
            if (result is JToken json)
            {
                converted = (T)json.ToObject(typeof(T));
            }
            else
            {
                converted = (T)result;
            }
            taskSource.TrySetResult(converted);
        }

        public void Cancel()
        {
            taskSource.TrySetException(new TimeoutException("Message response not recieved in 30 seconds"));
        }
    }
}
