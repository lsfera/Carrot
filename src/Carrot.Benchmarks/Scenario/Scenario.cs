using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Carrot.Messages;

namespace Carrot.Benchmarks.Scenario
{

    [Config(typeof(CustomConfig))]
    public abstract class Scenario
    {
        private readonly IBroker _broker;
        private readonly Exchange _exchange;
        private const Int32 Count = 10000;
        private protected const String QueueName = "queue";
        private protected const String RoutingKey = "routing_key";
        private protected const String ExchangeName = "exchange";
        private protected const String EndpointUrl = "amqp://guest:guest@rabbitmq:5672/";
        private readonly ManualResetEvent _event = new ManualResetEvent(false);
        
        protected Scenario(IBroker broker, Exchange exchange, Queue queue)
        {
            _broker = broker;
            _exchange = exchange;
            _broker.SubscribeByAtLeastOnce(queue, _ => { _.Consumes(new FooConsumer(Count, _event)); });
        }

        [Benchmark]
        public Task RunAsync()
        {
            var connection = _broker.Connect();
            var tasks = new Task[Count +1];
            
            for (var i = 0; i < Count; i++)
                tasks[i] =  connection.PublishAsync(BuildMessage(i), _exchange, RoutingKey);
            tasks[Count] = ConsumeAsync();

            return Task.WhenAll(tasks)
                .ContinueWith(_ =>
                {
                    for (var i = 0; i <= Count; i++)
                        tasks[i] = null;
                    tasks = null;

                    return Task.CompletedTask;
                }, TaskContinuationOptions.RunContinuationsAsynchronously)
                .Unwrap()
                .ContinueWith(_ =>
                    {
                        connection.Dispose();
                        return Task.CompletedTask;
                    }, 
                    TaskContinuationOptions.RunContinuationsAsynchronously)
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