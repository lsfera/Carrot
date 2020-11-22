using System;
using System.Reflection;

namespace Carrot.Configuration
{
    public class MessageBinding : IEquatable<MessageBinding>
    {
        public readonly String RawName;
        public readonly TypeInfo RuntimeType;
        public readonly TimeSpan? ExpiresAfter;

        public MessageBinding(String rawName, TypeInfo runtimeType, TimeSpan? expiresAfter = null)
        {
            RawName = rawName;
            RuntimeType = runtimeType;
            ExpiresAfter = expiresAfter;
        }

        public static Boolean operator ==(MessageBinding left, MessageBinding right) => Equals(left, right);

        public static Boolean operator !=(MessageBinding left, MessageBinding right) => !Equals(left, right);

        public Boolean Equals(MessageBinding other) 
        => other is not null &&
           (ReferenceEquals(this, other) || String.Equals(RawName, other.RawName));

        public override Boolean Equals(Object obj)
        => obj is not null &&
            (ReferenceEquals(this, obj) ||
            (obj is MessageBinding other && Equals(other)));

        public override Int32 GetHashCode() => RawName?.GetHashCode() ?? 0;
    }
}