using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mosquitto.Mqtt.Client.AspNetCore.Options;
using Mosquitto.Mqtt.Client.AspNetCore.Services;
using Mosquitto.Mqtt.Client.AspNetCore.Settings;
using MQTTnet.Client.Options;
using System;

namespace Mosquitto.Mqtt.Client.AspNetCore.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMqttClientHostedService(this IServiceCollection services)
        {
            services.AddMqttClientServiceWithConfig(aspOptionBuilder =>
            {
                var clientSettinigs = AppSettingsProvider.ClientSettings;
                var brokerHostSettings = AppSettingsProvider.BrokerHostSettings;

                aspOptionBuilder
                .WithCredentials(clientSettinigs.UserName, clientSettinigs.Password)
                .WithClientId(clientSettinigs.Id)
                .WithTcpServer(brokerHostSettings.Host, brokerHostSettings.Port);
            });
            return services;
        }

        private static IServiceCollection AddMqttClientServiceWithConfig(this IServiceCollection services, Action<MosquittoMqttClientOptionBuilder> configure)
        {
            services.AddSingleton<IMqttClientOptions>(serviceProvider =>
            {
                var optionBuilder = new MosquittoMqttClientOptionBuilder(serviceProvider);
                configure(optionBuilder);
                return optionBuilder.Build();
            });
            services.AddSingleton<MosquittoMqttClientService>();
            services.AddSingleton<IHostedService>(serviceProvider =>
            {
                return serviceProvider.GetService<MosquittoMqttClientService>();
            });

            return services;
        }
    }
}