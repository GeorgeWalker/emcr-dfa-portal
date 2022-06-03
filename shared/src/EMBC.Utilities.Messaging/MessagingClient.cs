﻿using System.Threading.Tasks;
using EMBC.ESS.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace EMBC.Utilities.Messaging
{
    public interface IMessagingClient
    {
        Task<TResponse?> Send<TResponse>(Query<TResponse> command);

        Task<string?> Send(Command command);
    }

    internal class MessagingClient : IMessagingClient
    {
        private readonly Dispatcher.DispatcherClient dispatcherClient;
        private readonly ILogger<MessagingClient> logger;

        public MessagingClient(Dispatcher.DispatcherClient dispatcherClient, ILogger<MessagingClient> logger)
        {
            this.dispatcherClient = dispatcherClient;
            this.logger = logger;
        }

        public async Task<string?> Send(Command command)
        {
            logger.LogDebug("Sending command {0}", command.GetType().FullName);
            var response = await dispatcherClient.DispatchAsync<string>(command);
            return response;
        }

        public async Task<TResponse?> Send<TResponse>(Query<TResponse> command)
        {
            logger.LogDebug("Sending query {0}", command.GetType().FullName);
            var response = await dispatcherClient.DispatchAsync<TResponse>(command);
            return response;
        }
    }
}
