using System;
using System.Threading.Tasks;
using Carrot.Logging;
using Carrot.Messages;

namespace Carrot.Extensions
{
    internal static class ConsumerExtensions
    {
        internal static AggregateConsumingResult HandleErrorResult(this Task<AggregateConsumingResult> task,
                                                                   ILog log)
        {
            var result = task.Result;

            switch (result)
            {
                case CorruptedMessageConsumingFailure _:
                    log.Error("message content corruption detected");
                    break;
                case UnresolvedMessageConsumingFailure _:
                    log.Error("runtime type cannot be resolved");
                    break;
                case UnsupportedMessageConsumingFailure _:
                    log.Error("message type cannot be resolved");
                    break; 
                case ConsumingFailureBase consumingFailureBase: 
                    consumingFailureBase.WithErrors(_ => log.Error("consuming error",
                        _.GetBaseException()));
                    break;
            }
            return result;
        }

        internal static Task<ConsumedMessage.ConsumingResult> SafeConsumeAsync(this IConsumer consumer,
                                                                               ConsumedMessageBase message,
                                                                               IOutboundChannel outboundChannel)
        {
            try
            {
                return consumer.ConsumeAsync(new ConsumingContext(message, outboundChannel))
                               .ContinueWith(_ =>
                                             {
                                                 if (_.Exception == null)
                                                     return new ConsumedMessage.Success(message, consumer);
                                             
                                                 return BuildFailure(consumer,
                                                                     message,
                                                                     _.Exception.GetBaseException());
                                             }, TaskContinuationOptions.RunContinuationsAsynchronously);
            }
            catch (Exception exception) { return Task.FromResult(BuildFailure(consumer, message, exception)); }
        }

        private static ConsumedMessage.ConsumingResult BuildFailure(IConsumer consumer,
                                                                    ConsumedMessageBase message,
                                                                    Exception exception)
        {
            consumer.OnError(exception);
            return new ConsumedMessage.Failure(message, consumer, exception);
        }
    }
}