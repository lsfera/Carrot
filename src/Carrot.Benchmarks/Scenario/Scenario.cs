using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Carrot.Messages;

namespace Carrot.Benchmarks.Scenario
{
    public abstract class Scenario
    {
        private readonly IBroker _broker;
        private readonly Exchange _exchange;
        private readonly Queue _queue;
        private const Int32 Count = 10000;
        private protected const String QueueName = "queue";
        private protected const String DurableQueueName = "durable_queue";
        private protected const String RoutingKey = "routing_key";
        private protected const String ExchangeName = "exchange";
        private protected const String DurableExchangeName = "durable_exchange";
        private protected const String EndpointUrl = "amqp://guest:guest@rabbitmq:5672/";
        private readonly ManualResetEvent _event = new ManualResetEvent(false);
        private IConnection _connection;

        protected Scenario(IBroker broker, Exchange exchange, Queue queue)
        {
            _broker = broker;
            _exchange = exchange;
            _queue = queue;
        }


        [GlobalSetup]
        public void Setup()
        {
            _broker.SubscribeByAtLeastOnce(_queue, _ => { _.Consumes(new FooConsumer(Count, _event)); });
            _connection = _broker.Connect();
        }

        [GlobalCleanup]
        public void TearDown()
        {
            _connection.Dispose();
        }

        [Benchmark]
        public Task RunAsync()
        {
            var tasks = new Task[Count +1];
            
            for (var i = 0; i < Count; i++)
                tasks[i] =  _connection.PublishAsync(BuildMessage(i), _exchange, RoutingKey);
            tasks[Count] = ConsumeAsync();

            return Task.WhenAll(tasks)
                .ContinueWith(_ =>
                {
                    for (var i = 0; i <= Count; i++)
                        tasks[i] = null;
                    tasks = null;

                    return Task.CompletedTask;
                }, TaskContinuationOptions.RunContinuationsAsynchronously)
                .Unwrap();
        }

        private Task ConsumeAsync()
        {
            _event.WaitOne();
            return Task.CompletedTask;
        }

        protected abstract OutboundMessage<Foo> BuildMessage(Int32 i);

    }
}