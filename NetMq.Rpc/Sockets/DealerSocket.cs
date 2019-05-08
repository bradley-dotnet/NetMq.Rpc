namespace NetMq.Rpc.Sockets
{
    public class DealerSocket : BaseSocket
    {
        public DealerSocket(string endpoint) : base(new NetMQ.Sockets.DealerSocket(endpoint))
        { }
    }
}
