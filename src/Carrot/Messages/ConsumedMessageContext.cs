using System;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Serialization;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class ConsumedMessageContext
    {
        public String Source => _args.Exchange;

        public String ContentType => _args.BasicProperties.ContentTypeOrDefault();

        public String ContentEncoding => _args.BasicProperties.ContentEncodingOrDefault();

        public String MessageType => _args.BasicProperties.Type;

        private readonly BasicDeliverEventArgs _args;

        private ConsumedMessageContext(BasicDeliverEventArgs args) => _args = args;

        public static ConsumedMessageContext FromBasicDeliverEventArgs(BasicDeliverEventArgs args)
        {
            Guard.AgainstNull(args, nameof(args));
            return new ConsumedMessageContext(args);
        }

        internal ConsumedMessage ToConsumedMessage(ISerializer serializer, MessageBinding messageBinding) 
        => new ConsumedMessage(serializer.Deserialize(_args.Body,
                    messageBinding.RuntimeType,
                    _args.BasicProperties.CreateEncoding()),
                _args);

        internal ISerializer CreateSerializer(SerializationConfiguration configuration) 
        => _args.BasicProperties.CreateSerializer(configuration);
    }
}