using Microsoft.Extensions.Hosting;
using Mosquitto.Mqtt.Client.AspNetCore.Client;
using MQTTnet.Client.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Mosquitto.Mqtt.Client.AspNetCore.Services
{
    public class MosquittoMqttClientService : IHostedService
    {
        private readonly MosquittoMqttClient client;

        public MosquittoMqttClientService(IMqttClientOptions options)
        {
            client = new MosquittoMqttClient(options);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return client.StartClientAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return client.StopClientAsync();
        }
    }
}