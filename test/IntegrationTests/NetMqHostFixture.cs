using System;
using Microsoft.Extensions.Options;
using Moq;

namespace Whitestone.Cambion.Transport.NetMQ.IntegrationTests
{
    public class NetMqHostFixture : IDisposable
    {
        public NetMqTransport Transport { get; }

        public NetMqHostFixture()
        {
            NetMqConfig config = new()
            {
                PublishAddress = "tcp://localhost:9990",
                SubscribeAddress = "tcp://localhost:9991",
                UseMessageHost = true
            };

            Mock<IOptions<NetMqConfig>> options = new();
            options.SetupGet(x => x.Value).Returns(config);

            Transport = new NetMqTransport(options.Object);

            Transport.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Transport.StopAsync().GetAwaiter().GetResult();
        }
    }
}
