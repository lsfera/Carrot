using System;

namespace Carrot.Messages.Replies
{
    internal static class ReplyConfigurationFactory
    {
        public static ReplyConfiguration Create(String exchangeType, String exchangeName, String routingKey) =>
            exchangeType.ToLowerInvariant() switch
            {
                "direct" => new DirectReplyConfiguration(exchangeName, routingKey),
                "topic" => new TopicReplyConfiguration(exchangeName, routingKey),
                "fanout" => new FanoutReplyConfiguration(exchangeName),
                _ => throw new ArgumentException($"Exchange type not recognized: {exchangeType}"),
            };
    }
}