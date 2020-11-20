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
            var nw = new Builder()
                .UseNetwork("benchmark").ReuseIfExist().Build();
            return new RabbitMqBroker(

                new Builder()
                    .UseContainer()
                    .UseImage("rabbitmq:3.8.9-management-alpine")
                    .ExposePort(5672, 5672)
                    .ExposePort(15672, 15672)
                    .Wait("rabbitmq", ReadyProbe)
                    .UseNetwork(nw)
                    .ReuseIfExists()
                    .WithEnvironment("NODENAME=rabbit1")
                    .WithHostName("rabbitmq")
                    .WithName("rabbitmq")
                    .Build()
                    .Start());
        }

        private static int ReadyProbe(IContainerService containerService, int arg2)
        {
            return containerService.Execute("rabbitmqctl --node rabbit1 await_startup ").Success ? 0 : 500;
        }

        private RabbitMqBroker(IContainerService service)
        {
            _service = service;
        }

        public void Dispose()
        {
            _service?.Dispose();
        }
    }
}