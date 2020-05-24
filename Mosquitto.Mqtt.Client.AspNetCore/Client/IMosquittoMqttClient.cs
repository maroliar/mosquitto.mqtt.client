using System.Threading.Tasks;

namespace Mosquitto.Mqtt.Client.AspNetCore.Client
{
    public interface IMosquittoMqttClient
    {
        Task StartClientAsync();
        Task StopClientAsync();
    }
}
