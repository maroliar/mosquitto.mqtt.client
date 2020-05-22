using MQTTnet.Client.Options;
using System;

namespace Mosquitto.Mqtt.Client.AspNetCore.Options
{
    public class MosquittoMqttClientOptionBuilder : MqttClientOptionsBuilder
    {
        public IServiceProvider ServiceProvider { get; }

        public MosquittoMqttClientOptionBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }        
    }
}
