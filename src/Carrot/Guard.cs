using System;
using System.Runtime.CompilerServices;

namespace Carrot
{
    public static class Guard
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AgainstNull(Object argumentValue, String argumentName)
        {
            if (argumentValue is null)
                throw new ArgumentNullException($"{argumentName} can't be null or empty.");
        }
    }
}