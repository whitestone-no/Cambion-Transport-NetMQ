using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using RandomTestValues;
using Xunit;

namespace Whitestone.Cambion.Transport.NetMQ.IntegrationTests
{
    public class NetMqTransportTests(NetMqHostFixture fixture) : IClassFixture<NetMqHostFixture>
    {
        [Fact]
        public async Task Publish_NullValue_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await fixture.Transport.PublishAsync(null); });
        }

        [Fact]
        public async Task PublishAsync_SameDataReceived_Success()
        {
            // Arrange

            ManualResetEvent mre = new(false);
            byte[] expectedBytes = RandomValue.Array<byte>();

            byte[] actualBytes = null;
            fixture.Transport.MessageReceived += (_, e) =>
            {
                actualBytes = e.MessageBytes;
                mre.Set();
            };

            // Act

            await fixture.Transport.PublishAsync(expectedBytes);

            // Assert

            bool eventFired = mre.WaitOne(TimeSpan.FromSeconds(5));

            Assert.True(eventFired);
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Fact]
        public async Task PublishOnHost_ReceiveOnNonHost_Success()
        {
            // Arrange

            NetMqConfig config = new()
            {
                PublishAddress = "tcp://localhost:9990",
                SubscribeAddress = "tcp://localhost:9991",
                UseMessageHost = false
            };

            Mock<IOptions<NetMqConfig>> options = new();
            options.SetupGet(x => x.Value).Returns(config);

            NetMqTransport transport = new(options.Object);

            await transport.StartAsync();

            byte[] expectedBytes = RandomValue.Array<byte>();

            ManualResetEvent mre = new(false);
            byte[] actualBytes = null;
            fixture.Transport.MessageReceived += (_, e) =>
            {
                actualBytes = e.MessageBytes;
                mre.Set();
            };

            // Act

            await fixture.Transport.PublishAsync(expectedBytes);

            // Assert

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.True(eventFired);
            Assert.Equal(expectedBytes, actualBytes);

            await transport.StopAsync();
        }
    }
}
