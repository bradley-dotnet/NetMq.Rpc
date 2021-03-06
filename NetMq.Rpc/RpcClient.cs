﻿using Microsoft.Extensions.Logging;
using NetMq.Rpc.Contracts;
using NetMq.Rpc.Models;
using NetMq.Rpc.Services;
using NetMq.Rpc.Sockets;
using NetMQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetMq.Rpc
{
    public class RpcClient<TContract> : IRpcClient<TContract>
        where TContract : class
    {
        private readonly ISocketFactory socketFactory;
        private readonly IMdpClientMessageFactory messageFactory;
        private readonly IRpcRequestManager rpcRequestManager;
        private readonly ILogger logger;

        private ISocket socket;

        public RpcClient(string endpoint, ILogger logger = null)
            : this(new SocketFactory<DealerSocket>(endpoint),
                  new MdpClientMessageFactory(true),
                  new RpcRequestManager(new TimerFactory(), new SystemDateTimeProvider()),
                  logger)
        {
            if (!typeof(TContract).IsAssignableFrom(GetType()))
            {
                throw new InvalidOperationException("Client must implement contract interface");
            }
        }

        public RpcClient(ISocketFactory socketFactory,
            IMdpClientMessageFactory messageFactory,
            IRpcRequestManager rpcRequestManager,
            ILogger logger)
        {
            this.socketFactory = socketFactory;
            this.messageFactory = messageFactory;
            this.rpcRequestManager = rpcRequestManager;
            this.logger = logger;

            Reconnect();
        }

        public void Dispose()
        {
            socket?.Dispose();
        }

        protected async void ExecuteFireAndForget(object[] parameters, [CallerMemberName]string methodName = "")
        {
            await SendRequest(methodName, parameters);
        }

        protected void ExecuteAndBlock(object[] parameters, [CallerMemberName]string methodName = "")
        {
            SendRequest(methodName, parameters).Wait();
        }

        protected Task ExecuteAsync(object[] parameters, [CallerMemberName]string methodName = "")
        {
            return SendRequest(methodName, parameters);
        }

        protected T GetReturnValue<T>(object[] parameters, [CallerMemberName]string methodName = "")
        {
            return SendRequest<T>(methodName, parameters).Result;
        }

        protected Task<T> GetReturnValueAsync<T>(object[] parameters, [CallerMemberName]string methodName = "")
        {
            return SendRequest<T>(methodName, parameters);
        }

        protected object[] MakeParams(params object[] parameters)
        {
            return parameters;
        }

        private void Reconnect()
        {
            try
            {
                if (socket != null)
                {
                    socket.Dispose();
                }

                socket = socketFactory.Create();

                socket.MessageReady += ParseMessage;
            }
            catch (Exception)
            {
                logger?.LogError("Unable to connect to broker");
            }
        }

        private void ParseMessage()
        {
            var message = socket.GetNextMessage();
            var protocol = message.ElementAt(1).ConvertToString();
            if (protocol == MdpProtocolNames.Client)
            {
                HandleResponse(message.Skip(3));
            }
        }

        private Task SendRequest(string methodName, object[] parameters)
        {
            var syncId = SendRequestMessage(methodName, parameters);
            var resultTask = rpcRequestManager.TrackRequest(syncId);
            return resultTask;
        }

        private Task<T> SendRequest<T>(string methodName, object[] parameters)
        {
            var syncId = SendRequestMessage(methodName, parameters);
            var resultTask = rpcRequestManager.TrackRequest<T>(syncId);
            return resultTask;
        }

        private Guid SendRequestMessage(string methodName, object[] parameters)
        {
            logger?.LogDebug("Sending request for method {methodName}", methodName);
            var message = new RpcMessage
            {
                SynchronizationId = Guid.NewGuid(),
                MethodName = methodName,
                Parameters = parameters
            };
            var serializedMessage = JsonConvert.SerializeObject(message);
            var requestBytes = Encoding.UTF8.GetBytes(serializedMessage);
            socket.SendMessage(messageFactory.GenerateRequest(typeof(TContract).Name, new byte[][] { requestBytes }));
            return message.SynchronizationId;
        }

        private void HandleResponse(IEnumerable<NetMQFrame> frames)
        {
            logger?.LogDebug("Received request response");
            var body = frames.ElementAt(0);

            var message = JsonConvert.DeserializeObject<RpcResponse>(body.ConvertToString());
            rpcRequestManager.SetRequestResult(message.SynchronizationId, message.ReturnValue);
        }
    }
}
