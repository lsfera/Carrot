using System;
using System.Text;
using Carrot.Configuration;
using Carrot.Serialization;
using RabbitMQ.Client;

namespace Carrot.Extensions
{
    internal static class BasicPropertiesExtensions
    {
        internal static ISerializer CreateSerializer(this IBasicProperties source,
                                                     SerializationConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            return configuration.Create(source.ContentTypeOrDefault());
        }

        internal static String ContentTypeOrDefault(this IBasicProperties source,
                                                    String @default = "application/json")
        {
            const String key = "Content-Type";

            if (!String.IsNullOrEmpty(source.ContentType))
                return source.ContentType;

            if (source.Headers == null || !source.Headers.ContainsKey(key))
                return @default;

            var bytes = (Byte[])source.Headers[key];

            return bytes.Length > 0 ? source.CreateEncoding().GetString(bytes) : @default;
        }

        internal static Encoding CreateEncoding(this IBasicProperties source) 
        => Encoding.GetEncoding(source.ContentEncodingOrDefault());

        internal static String ContentEncodingOrDefault(this IBasicProperties source,
                                                        String @default = "UTF-8") 
        => source.ContentEncoding ?? @default;
    }
}