using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.NetMQ
{
    public static class NetMqTransportExtensions
    {
        public static ICambionSerializerBuilder UseNetMqTransport(this ICambionTransportBuilder builder, Action<NetMqConfig> configure)
        {
            return UseNetMqTransport(builder, null, configure, null);
        }

        public static ICambionSerializerBuilder UseNetMqTransport(this ICambionTransportBuilder builder, IConfiguration configuration, string cambionConfigurationKey = "Cambion")
        {
            return UseNetMqTransport(builder, configuration, null, cambionConfigurationKey);
        }

        public static ICambionSerializerBuilder UseNetMqTransport(this ICambionTransportBuilder builder, IConfiguration configuration, Action<NetMqConfig> configure, string cambionConfigurationKey = "Cambion")
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ITransport), typeof(NetMqTransport), ServiceLifetime.Singleton));

            if (configuration != null)
            {
                string assemblyName = typeof(NetMqTransport).Assembly.GetName().Name;

                IConfigurationSection config = configuration.GetSection(cambionConfigurationKey).GetSection("Transport").GetSection(assemblyName);

                if (config.Exists())
                {
                    builder.Services.Configure<NetMqConfig>(config);
                }
            }

            builder.Services.AddOptions<NetMqConfig>()
                .Configure(conf => { configure?.Invoke(conf); });

            return (ICambionSerializerBuilder)builder;
        }
    }
}