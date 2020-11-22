using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Fallback
{
    public class DeadLetterStrategy : IFallbackStrategy
    {
        private readonly Exchange _exchange;

        private DeadLetterStrategy(Exchange exchange) => _exchange = exchange;

        public static IFallbackStrategy New(IBroker broker, Queue queue) 
        => New(broker, queue, _ => $"{_}::dle");

        public static IFallbackStrategy New(IBroker broker,
                                            Queue queue,
                                            Func<String, String> exchangeNameBuilder) 
        => new DeadLetterStrategy(broker.DeclareDurableDirectExchange(exchangeNameBuilder(queue.Name)));

        public Task<IFallbackApplied> Apply(IOutboundChannel channel, ConsumedMessageBase message) 
        => channel.ForwardAsync(message, _exchange, String.Empty)
                .ContinueWith(_ =>
                    _.Result is FailurePublishing
                        ? new FallbackAppliedFailure(_.Exception?.GetBaseException()) as IFallbackApplied
                        : new FallbackAppliedSuccessful(), TaskContinuationOptions.RunContinuationsAsynchronously);
    }
}