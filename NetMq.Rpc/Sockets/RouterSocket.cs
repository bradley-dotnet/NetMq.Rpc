namespace NetMq.Rpc.Sockets
{
    public class RouterSocket : BaseSocket
    {
        public RouterSocket(string endpoint) : base(new NetMQ.Sockets.RouterSocket(endpoint))
        { }
    }
}
