using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Serialization;

namespace Carrot.Configuration
{
    public class SerializationConfiguration
    {
        internal const String DefaultContentType = "application/json";
        internal const String DefaultContentEncoding = "UTF-8";

        private readonly IDictionary<Predicate<ContentNegotiator.MediaTypeHeader>, ISerializer> _serializers =
            new Dictionary<Predicate<ContentNegotiator.MediaTypeHeader>, ISerializer>
                {
                    { _ => _.MediaType == DefaultContentType, new JsonSerializer() }
                };

        private IContentNegotiator _negotiator = new ContentNegotiator();

        internal SerializationConfiguration() { }

        public void Map(Predicate<ContentNegotiator.MediaTypeHeader> predicate, ISerializer serializer)
        {
            Guard.AgainstNull(predicate, nameof(predicate));
            Guard.AgainstNull(serializer, nameof(serializer));
            
            _serializers.Add(predicate, serializer);
        }

        public void NegotiateBy(IContentNegotiator negotiator)
        {
            Guard.AgainstNull(negotiator, nameof(negotiator));
            _negotiator = negotiator;
        }

        internal virtual ISerializer Create(String contentType)
        {
            Guard.AgainstNull(contentType, nameof(contentType));
            
            var result = _negotiator.Negotiate(contentType);

            foreach (var serializer in 
                from header in result 
                from serializer in _serializers 
                    where serializer.Key(header) select serializer)
                return serializer.Value;

            return NullSerializer.Instance;
        }
    }
}