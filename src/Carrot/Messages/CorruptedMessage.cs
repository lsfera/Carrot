using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class CorruptedMessage : NonConsumableMessage
    {
        internal CorruptedMessage(BasicDeliverEventArgs args)
            : base(args) 
        {
        }

        protected override ConsumingFailureBase Result()
        {
            return new CorruptedMessageConsumingFailure(this);
        }
    }
}