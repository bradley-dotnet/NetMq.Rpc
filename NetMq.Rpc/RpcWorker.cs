using Microsoft.Extensions.Logging;
using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using NetMq.Rpc.Services;
using NetMq.Rpc.Sockets;
using NetMQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMq.Rpc
{
    public abstract class RpcWorker<TContract> : IRpcWorker<TContract>
        where TContract : class
    {
        private readonly IMethodCache methodCache;
        private readonly ISocketFactory socketFactory;
        private readonly IMdpWorkerMessageFactory messageFactory;
        private readonly IMethodInvoker methodInvoker;
        private readonly ILogger logger;

        private ITimer brokerLifetime;
        private ISocket socket;

        public RpcWorker(string endpoint, ILogger logger = null) :
            this(new MethodCacheFactory(),
                new SocketFactory<DealerSocket>(endpoint),
                new MdpWorkerMessageFactory(),
                new TimerFactory(),
                new MethodInvoker(),
                logger)
        {
        }

        public RpcWorker(IMethodCacheFactory methodCacheFactory,
            ISocketFactory socketFactory,
            IMdpWorkerMessageFactory messageFactory,
            ITimerFactory timerFactory,
            IMethodInvoker methodInvoker,
            ILogger logger)
        {
            if (!GetType().IsAssignableFrom(typeof(TContract)))
            {
                throw new InvalidOperationException("Worker must implement contract interface");
            }

            this.socketFactory = socketFactory;
            methodCache = methodCacheFactory.ConstructCache(typeof(TContract));
            this.messageFactory = messageFactory;
            this.methodInvoker = methodInvoker;
            this.logger = logger;

            brokerLifetime = timerFactory.Create(TimeSpan.FromSeconds(5), Reconnect);
            brokerLifetime.Start();

            Reconnect();
        }

        public void Dispose()
        {
            socket?.SendMessage(messageFactory.GenerateDisconnect());
            socket?.Dispose();
        }

        private void ParseMessage()
        {
            var message = socket.GetNextMessage();
            var protocol = message.ElementAt(1).ConvertToString();
            if (protocol == MdpProtocolNames.Worker)
            {
                var command = (MdpWorkerProtocol)message.ElementAt(2).Buffer[0]; ;
                switch (command)
                {
                    case MdpWorkerProtocol.Request:
                        HandleRequest(message.Skip(3));
                        break;
                    case MdpWorkerProtocol.Heartbeat:
                        brokerLifetime.Reset(); // Start the 5 second timer over
                        break;
                    case MdpWorkerProtocol.Disconnect:
                        Reconnect();
                        break;
                    default:
                        break;
                }
            }
        }

        private void SendReady()
        {
            socket.SendMessage(messageFactory.GenerateReady(typeof(TContract).Name));
        }

        private async void HandleRequest(IEnumerable<NetMQFrame> frames)
        {
            var clientAddress = frames.ElementAt(0);
            var body = frames.ElementAt(2);

            var message = JsonConvert.DeserializeObject<RpcMessage>(body.ConvertToString());
            var returnValue = await methodInvoker.GetMethodResult(this, methodCache.GetMethod(message.MethodName), message.Parameters);

            var reply = new RpcResponse { ReturnValue = returnValue, SynchronizationId = message.SynchronizationId };
            var replyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reply));
            socket.SendMessage(messageFactory.GenerateReply(clientAddress.Buffer, new byte[][] { replyBytes }));
        }

        private void Reconnect()
        {
            if (socket != null)
            {
                socket.Dispose();
            }

            socket = socketFactory.Create();

            socket.MessageReady += ParseMessage;
            socket.AddTimer(TimeSpan.FromSeconds(1), GenerateHeartbeats);
            SendReady();
        }

        private void GenerateHeartbeats()
        {
            socket.SendMessage(messageFactory.GenerateHeartbeat());
        }
    }
}
