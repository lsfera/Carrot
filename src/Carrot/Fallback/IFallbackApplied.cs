using System;

namespace Carrot.Fallback
{
    public interface IFallbackApplied
    {
        Boolean Success { get; }
    }

    internal class FallbackAppliedSuccessful : IFallbackApplied
    {
        public Boolean Success => true;
    }

    internal class FallbackAppliedFailure : IFallbackApplied
    {
        public FallbackAppliedFailure(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
        public Boolean Success => false;
    }
}