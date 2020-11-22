using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class Connection : IConnection
    {
        protected readonly EnvironmentConfiguration Configuration;

        private readonly RabbitMQ.Client.IConnection _connection;
        private readonly IEnumerable<ConsumerBase> _consumers;
        private readonly IOutboundChannel _outboundChannel;

        internal Connection(RabbitMQ.Client.IConnection connection,
                            IEnumerable<ConsumerBase> consumers,
                            IOutboundChannel outboundChannel,
                            EnvironmentConfiguration configuration)
        {
            _connection = connection;
            _connection.ConnectionShutdown += OnConnectionShutdown;
            _consumers = consumers;
            _outboundChannel = outboundChannel;
            Configuration = configuration;
        }
        
        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                           Exchange exchange,
                                                           String routingKey = "")
            where TMessage : class
        {
            return _outboundChannel.PublishAsync(message, exchange, routingKey);
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers)
                consumer.Dispose();

            _outboundChannel?.Dispose();
            _connection.ConnectionShutdown -= OnConnectionShutdown;
            Cleanup(_connection, 200, "Connection Disposed");
            _connection?.Dispose();
        }

        void OnConnectionShutdown(Object connection, ShutdownEventArgs reason) 
        => Cleanup(_connection, reason.ReplyCode, reason.ReplyText);

        private static void Cleanup(RabbitMQ.Client.IConnection connection, UInt16 replyCode = 200, String message = "Unknown")
        {
            if (connection == null) return;
            try
            {
                if (connection.IsOpen)
                    connection.Close(replyCode, message);
            }
            catch
            {
            }
        }
    }
}