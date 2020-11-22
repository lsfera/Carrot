using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Carrot.Serialization
{
    public static class EncodingExtensions
    {
        public static String GetString(this Encoding encoding, ReadOnlyMemory<Byte> memory)
        {
            var arraySegment = GetArray(memory);
            return encoding.GetString(arraySegment.Array ?? throw new InvalidOperationException(), 
                                 arraySegment.Offset, arraySegment.Count);
        }

        private static ArraySegment<Byte> GetArray(ReadOnlyMemory<Byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
                throw new InvalidOperationException("Buffer backed by array was expected");
            return result;
        }
    }
}