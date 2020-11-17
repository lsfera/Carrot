using System;
using System.Threading;
using System.Threading.Tasks;

namespace Carrot.Benchmarks
{
    internal sealed class FooConsumer : Consumer<Foo>
    {
        private readonly Int32 _expectedCount;
        private readonly ManualResetEvent _event;
        private volatile Int32 _accumulator;
        
        public FooConsumer(Int32 expectedCount, ManualResetEvent @event)
        {
            _expectedCount = expectedCount;
            _event = @event;
        }

        public override Task ConsumeAsync(ConsumingContext<Foo> context)
        {
            return Task.FromResult(0);
        }

        public override void OnConsumeCompletion()
        {
            base.OnConsumeCompletion();

            var value = Interlocked.Increment(ref _accumulator);
            if (value % _expectedCount == 0) _event.Set();
        }
    }
}