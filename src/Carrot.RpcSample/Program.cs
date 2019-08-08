﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Carrot.Messages.Replies;

namespace Carrot.RpcSample
{
    public class Program
    {
        private static void Main(String[] args)
        {
            const String routingKey = "request_routing_key";
            const String endpointUrl = "amqp://guest:guest@localhost:5672/";
            const String replyQueueName = "reply_to_queue";
            const String replyExchangeName = "reply_exchange";

            IMessageTypeResolver resolver = new MessageBindingResolver(typeof(Response).GetTypeInfo().Assembly);

            var broker = Broker.New(_ =>
                                    {
                                        _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                                        _.ResolveMessageTypeBy(resolver);
                                    });

            var exchange = broker.DeclareDirectExchange("request_exchange");
            var queue = broker.DeclareQueue("request_queue");
            broker.DeclareExchangeBinding(exchange, queue, routingKey);
            broker.SubscribeByAtLeastOnce(queue, _ => _.Consumes(new RequestConsumer(endpointUrl)));

            var replyExchange = broker.DeclareDirectExchange(replyExchangeName);
            var replyQueue = broker.DeclareQueue(replyQueueName);
            broker.DeclareExchangeBinding(replyExchange, replyQueue, replyQueueName);
            broker.SubscribeByAtLeastOnce(replyQueue, _ => _.Consumes(new ResponseConsumer()));

            var connection = broker.Connect();

            var message = new OutboundMessage<Request>(new Request { Bar = 42 });
            message.SetCorrelationId(Guid.NewGuid().ToString());
            message.SetReply(new DirectReplyConfiguration(replyExchangeName, replyQueueName));
            connection.PublishAsync(message, exchange, routingKey);

            Console.ReadLine();
            connection.Dispose();
        }
    }

    internal class RequestConsumer : Consumer<Request>, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IBroker _broker;
        private readonly IMessageTypeResolver _resolver = new MessageBindingResolver(typeof(Response).GetTypeInfo().Assembly);

        public RequestConsumer(String endpointUrl)
        {
            _broker = Broker.New(_ =>
                                 {
                                     _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                                     _.ResolveMessageTypeBy(_resolver);
                                 });

            _connection = _broker.Connect();
        }

        public override Task ConsumeAsync(ConsumingContext<Request> context)
        {
            return Task.Factory
                       .StartNew(() =>
                                 {
                                     Console.WriteLine("[{0}]received '{1}' by '{2}' with correlation id {3}",
                                                       context.Message.ConsumerTag,
                                                       context.Message.Headers.MessageId,
                                                       GetType().Name,
                                                       context.Message.Headers.CorrelationId);

                                     var exchange = _broker.DeclareDirectExchange(context.Message
                                                                                         .Headers
                                                                                         .ReplyConfiguration
                                                                                         .ExchangeName);
                                     var queue = _broker.DeclareQueue(context.Message
                                                                             .Headers
                                                                             .ReplyConfiguration
                                                                             .RoutingKey);
                                     _broker.DeclareExchangeBinding(exchange,
                                                                    queue,
                                                                    context.Message
                                                                           .Headers
                                                                           .ReplyConfiguration.RoutingKey);

                                     var outboundMessage = new OutboundMessage<Response>(new Response
                                                                                             {
                                                                                                 BarBar = context.Message
                                                                                                                 .Content
                                                                                                                 .Bar * 2
                                                                                             });
                                     outboundMessage.SetCorrelationId(context.Message
                                                                             .Headers
                                                                             .CorrelationId);
                                     _connection.PublishAsync(outboundMessage,
                                                              exchange,
                                                              context.Message
                                                                     .Headers
                                                                     .ReplyConfiguration.RoutingKey);
                                 });
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

    internal class ResponseConsumer : Consumer<Response>
    {
        public override Task ConsumeAsync(ConsumingContext<Response> context)
        {
            return Task.Factory
                       .StartNew(() =>
                                 {
                                     Console.WriteLine("[{0}]received '{1}' by '{2}' with correlation id {3}",
                                                       context.Message.ConsumerTag,
                                                       context.Message.Headers.MessageId,
                                                       GetType().Name,
                                                       context.Message.Headers.CorrelationId);
                                 });
        }
    }
}