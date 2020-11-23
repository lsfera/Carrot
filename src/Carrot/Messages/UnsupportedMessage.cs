using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public sealed class UnsupportedMessage : NonConsumableMessage
    {
        internal UnsupportedMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        protected override ConsumingFailureBase Result(ConsumedMessage.ConsumingResult[] results) 
        => new UnsupportedMessageConsumingFailure(this, results);
    }
}