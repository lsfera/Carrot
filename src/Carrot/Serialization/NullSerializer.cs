using System;
using System.Text;

namespace Carrot.Serialization
{
    public class NullSerializer : ISerializer
    {
        internal static readonly ISerializer Instance = new NullSerializer();

        private NullSerializer() { }

        public Object Deserialize(Byte[] body, Type type, Encoding encoding = null)
        {
            return null;
        }

        public String Serialize(Object obj)
        {
            return null;
        }
    }
}