using System;
using Carrot.Logging;
using RabbitMQ.Client;
// ReSharper disable UnusedMember.Global

namespace Carrot.Configuration
{
    public class EnvironmentConfiguration
    {
        private IMessageTypeResolver _resolver;

        internal EnvironmentConfiguration() { }

        internal Uri EndpointUri { get; private set; }

        internal IMessageTypeResolver MessageTypeResolver
        {
            get => _resolver ?? DefaultMessageTypeResolver.Instance;
            private set => _resolver = value;
        }

        internal UInt32 PrefetchSize { get; private set; }

        internal UInt16 PrefetchCount { get; private set; }

        internal INewId IdGenerator { get; private set; } =  new NewGuid();

        internal ILog Log { get; private set; } = new DefaultLog();

        internal SerializationConfiguration SerializationConfiguration { get; } = new SerializationConfiguration();

        internal Func<IModel, EnvironmentConfiguration, IOutboundChannel> OutboundChannelBuilder { get; private set; } = OutboundChannel.Default();

        public void Endpoint(Uri uri)
        {
            Guard.AgainstNull(uri, nameof(uri));
            EndpointUri = uri;
        }

        public void ResolveMessageTypeBy(IMessageTypeResolver messageTypeResolver)
        {
            Guard.AgainstNull(messageTypeResolver, nameof(messageTypeResolver));
            MessageTypeResolver = messageTypeResolver;
        }

        public void SetPrefetchSize(UInt32 value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            PrefetchSize = value;
        }

        public void SetPrefetchCount(UInt16 value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            PrefetchCount = value;
        }

        public void GeneratesMessageIdBy(INewId instance)
        {
            Guard.AgainstNull(instance, nameof(instance));
            IdGenerator = instance;
        }
            
        public void LogBy(ILog log)
        {
            Guard.AgainstNull(log, nameof(log));
            Log = log;
        }

        public void ConfigureSerialization(Action<SerializationConfiguration> configure)
        {
            Guard.AgainstNull(configure, nameof(configure));
            configure(SerializationConfiguration);
        }

        public void PublishBy(Func<IModel, EnvironmentConfiguration, IOutboundChannel> builder)
        {
            Guard.AgainstNull(builder, nameof(builder));
            OutboundChannelBuilder = builder;
        }
    }
}