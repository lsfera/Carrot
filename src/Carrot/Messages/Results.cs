using System;
using Carrot.Extensions;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    #region publishing

    public interface IPublishResult { }

    public class FailurePublishing : IPublishResult
    {
        public readonly Exception Exception;

        internal FailurePublishing(Exception exception) => Exception = exception;
    }

    public class SuccessfulPublishing : IPublishResult
    {
        public readonly String MessageId;
        public readonly Int64 Timestamp;

        private SuccessfulPublishing(String messageId, Int64 timestamp)
        {
            MessageId = messageId;
            Timestamp = timestamp;
        }

        internal static SuccessfulPublishing FromBasicProperties(IBasicProperties properties) => 
            new SuccessfulPublishing(properties.MessageId, properties.Timestamp.UnixTime);
    }

    #endregion

    public abstract class AggregateConsumingResult
    {
        internal readonly ConsumedMessageBase Message;
        protected readonly ConsumedMessage.ConsumingResult[] Results;

        protected AggregateConsumingResult(ConsumedMessageBase message, ConsumedMessage.ConsumingResult[] results)
        {
            Message = message;
            Results = results;
        }

        internal virtual AggregateConsumingResult Reply(IInboundChannel inboundChannel,
                                                        IOutboundChannel outboundChannel)
        {
            Message.Acknowledge(inboundChannel);
            return this;
        }

        internal void NotifyConsumingCompletion() => Results.ForEach(_ => _.NotifyConsumingCompletion());

        internal void NotifyConsumingFault(Exception e) => Results.ForEach(_ => _.NotifyConsumingFault(e));
    }

    public class Success : AggregateConsumingResult
    {
        internal Success(ConsumedMessageBase message, ConsumedMessage.ConsumingResult[] results)
            : base(message, results)
        {
        }
    }

    public abstract class ConsumingFailureBase : AggregateConsumingResult
    {
        private readonly Exception[] _exceptions;

        protected ConsumingFailureBase(ConsumedMessageBase message,
                                       ConsumedMessage.ConsumingResult[] results,
                                       params Exception[] exceptions)
            : base(message, results) => _exceptions = exceptions;

        internal Exception[] Exceptions => _exceptions ?? Array.Empty<Exception>();

        internal void WithErrors(Action<Exception> action)
        {
            Guard.AgainstNull(action, nameof(action));
           
            Exceptions.NotNull()
                      .ForEach(action);
        }
    }

    public class ReiteratedConsumingFailure : ConsumingFailureBase
    {
        internal ReiteratedConsumingFailure(ConsumedMessageBase message,
                                            ConsumedMessage.ConsumingResult[] results,
                                            params Exception[] exceptions)
            : base(message, results, exceptions)
        {
        }

        internal override AggregateConsumingResult Reply(IInboundChannel inboundChannel,
                                                         IOutboundChannel outboundChannel) =>
            this;
    }

    public class ConsumingFailure : ConsumingFailureBase
    {
        internal ConsumingFailure(ConsumedMessageBase message,
                                  ConsumedMessage.ConsumingResult[] results,
                                  params Exception[] exceptions)
            : base(message, results, exceptions)
        {
        }

        internal override AggregateConsumingResult Reply(IInboundChannel inboundChannel,
                                                         IOutboundChannel outboundChannel)
        {
            Message.Requeue(inboundChannel);
            return this;
        }
    }

    internal class UnsupportedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnsupportedMessageConsumingFailure(ConsumedMessageBase message,
                                                    ConsumedMessage.ConsumingResult[] results)
            : base(message, results)
        {
        }
    }

    internal class UnresolvedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnresolvedMessageConsumingFailure(ConsumedMessageBase message,
                                                   ConsumedMessage.ConsumingResult[] results)
            : base(message, results)
        {
        }
    }

    internal class CorruptedMessageConsumingFailure : ConsumingFailureBase
    {
        internal CorruptedMessageConsumingFailure(ConsumedMessageBase message,
                                                  ConsumedMessage.ConsumingResult[] results)
            : base(message, results)
        {
        }
    }
}