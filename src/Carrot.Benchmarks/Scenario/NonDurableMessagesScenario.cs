using System;
using System.Reflection;
using Carrot.Configuration;
using Carrot.Messages;

namespace Carrot.Benchmarks.Scenario
{
    public class NonDurableMessagesScenario : Scenario
    {
        public NonDurableMessagesScenario()
            : base(Bootstrap.broker, Bootstrap.exchange, Bootstrap.queue)
        {
        }

        private static IBroker BuildBroker() =>
            Broker.New(_ =>
            {
                _.Endpoint(new Uri(EndpointUrl, UriKind.Absolute));
                _.ResolveMessageTypeBy(new MessageBindingResolver(typeof(Foo).GetTypeInfo().Assembly));
            });

        private static Exchange DeclareExchange(IBroker broker) => broker.DeclareDurableDirectExchange(ExchangeName);

        private static Queue BindQueue(IBroker broker, Exchange exchange)
        {
            var queue = broker.DeclareQueue(QueueName);
            broker.TryDeclareExchangeBinding(exchange, queue, RoutingKey);
            return queue;
        }

        private static (IBroker broker, Exchange exchange, Queue queue) Bootstrap
        {
            get
            {
                var broker = BuildBroker();
                var exchange = DeclareExchange(broker);
                var queue = BindQueue(broker, exchange);
                return (broker, exchange, queue);
            }
        }
        
        protected override OutboundMessage<Foo> BuildMessage(Int32 i)
        {
            return new OutboundMessage<Foo>(new Foo { Bar = i });
        }
    }
}