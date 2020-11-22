using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public abstract class NonConsumableMessage : ConsumedMessageBase
    {
        protected internal NonConsumableMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        internal override Object Content => null;

        internal override Task<AggregateConsumingResult> ConsumeAsync(IEnumerable<IConsumer> subscriptions,
                                                                      IOutboundChannel outboundChannel) 
        => Task.FromResult<AggregateConsumingResult>(Result(Array.Empty<ConsumedMessage.ConsumingResult>()));

        internal override Boolean Match(Type type) => false;

        protected abstract ConsumingFailureBase Result(ConsumedMessage.ConsumingResult[] results);
    }
}