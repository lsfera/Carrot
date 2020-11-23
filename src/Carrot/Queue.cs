using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public readonly struct Queue
    {
        public readonly String Name;
        internal readonly Boolean IsDurable;
        internal readonly IDictionary<String, Object> Arguments;

        internal Queue(String name,
                       Boolean isDurable = false,
                       IDictionary<String, Object> arguments = null)
        {
            Guard.AgainstNull(name, nameof(name));
            Name = name;
            IsDurable = isDurable;
            Arguments = arguments ?? new Dictionary<String, Object>();
        }

        public static Boolean operator ==(Queue left, Queue right) => left.Equals(right);

        public static Boolean operator !=(Queue left, Queue right) => !left.Equals(right);

        private Boolean Equals(Queue other) => String.Equals(Name, other.Name);

        public override Boolean Equals(Object obj) => obj is not null && obj is Queue queue && Equals(queue);

        public override Int32 GetHashCode() => Name.GetHashCode();

        internal void Declare(IModel model) 
        => model.QueueDeclare(Name,
                IsDurable,
                false,
                false,
                Arguments);
    }
}