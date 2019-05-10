using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc.Contracts
{
    public interface IRpcRequestManager
    {
        event Action ReconnectNeeded;

        Task TrackRequest(Guid syncId);
        Task<T> TrackRequest<T>(Guid syncId);
        void SetRequestResult(Guid syncId, object result);
    }
}
