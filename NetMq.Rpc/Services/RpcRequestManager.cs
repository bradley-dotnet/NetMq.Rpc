using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc.Services
{
    internal class RpcRequestManager : IRpcRequestManager
    {
        public event Action ReconnectNeeded;

        private readonly IDateTimeProvider dateTime;
        private ITimer timeoutTimer;
        private Dictionary<Guid, ITrackedRequest> pendingRequests = new Dictionary<Guid, ITrackedRequest>();

        public RpcRequestManager(ITimerFactory timerFactory,
            IDateTimeProvider dateTime)
        {
            this.dateTime = dateTime;
            timeoutTimer = timerFactory.Create(TimeSpan.FromSeconds(5), CheckDeadRequests);
            timeoutTimer.Start();
        }

        public void SetRequestResult(Guid syncId, object result)
        {
            if (pendingRequests.TryGetValue(syncId, out ITrackedRequest request))
            {
                request.SetResult(result);
                pendingRequests.Remove(syncId);
            }
        }

        public Task TrackRequest(Guid syncId)
        {
            var request = new TrackedRequest<object>(dateTime.Now);
            pendingRequests.Add(syncId, request);
            return request.Task;
        }

        public Task<T> TrackRequest<T>(Guid syncId)
        {
            var request = new TrackedRequest<T>(dateTime.Now);
            pendingRequests.Add(syncId, request);
            return request.Task;
        }

        private void CheckDeadRequests()
        {
            var oldestRequest = pendingRequests.Values.OrderBy(r => r.RequestTime).FirstOrDefault();
            if (oldestRequest != null && oldestRequest.RequestTime.AddSeconds(30) < dateTime.Now)
            {
                foreach (var request in pendingRequests.Values)
                {
                    request.Cancel();
                }
                pendingRequests.Clear();
                ReconnectNeeded?.Invoke();
            }
        }
    }
}
