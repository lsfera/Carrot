using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public readonly struct Exchange
    {
        public readonly String Name;
        internal readonly String Type;
        internal readonly Boolean IsDurable;
        internal readonly IDictionary<String, Object> Arguments;

        internal Exchange(String name,
                          String type,
                          Boolean isDurable = false,
                          IDictionary<String, Object> arguments = null)
        {
            Guard.AgainstNull(name,nameof(name));
            Guard.AgainstNull(type, nameof(type));
            Name = name;
            Type = type;
            IsDurable = isDurable;
            Arguments = arguments;
        }

        public static Boolean operator ==(Exchange left, Exchange right) => left.Equals(right);

        public static Boolean operator !=(Exchange left, Exchange right) => !left.Equals(right);

        private Boolean Equals(Exchange other) => String.Equals(Name, other.Name);

        public override Boolean Equals(Object obj) => obj is not null && obj is Exchange exchange && Equals(exchange);

        public override Int32 GetHashCode() => HashCode.Combine(Name);

        internal void Declare(IModel model) => model.ExchangeDeclare(Name, Type, IsDurable, false, Arguments);
    }
}