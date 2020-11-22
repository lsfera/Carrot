using System;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;

namespace Carrot.Benchmarks
{
    public class RabbitMqBroker : IDisposable
    {
        private readonly IContainerService _service;

        public static IDisposable Start()
        {
            return new RabbitMqBroker(
                new Builder().UseContainer()
                    .UseImage("rabbitmq:3.8.9-management-alpine")
                    .ExposePort(5672, 5672)
                    .ExposePort(15672, 15672)
                    .Wait("rabbitmq", ReadyProbe)
                    .ReuseIfExists()
                    .WithName("rabbitmq")
                    .WithEnvironment("NODENAME=rabbit1")
                    .Build()
                    .Start());
        }

        private static Int32 ReadyProbe(IContainerService containerService, Int32 arg2)
        {
            return containerService.Execute("rabbitmqctl --node rabbit1 await_startup ").Success ? 0 : 500;
        }

        private RabbitMqBroker(IContainerService service)
        {
            _service = service;
        }

        public void Dispose() => _service?.Dispose();
    }
}