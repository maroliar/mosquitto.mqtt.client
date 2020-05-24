using Mosquitto.Mqtt.Client.AspNetCore.Entities;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mosquitto.Mqtt.Client.AspNetCore.Client
{
    public class MosquittoMqttClient : IMosquittoMqttClient
    {
        const string TOPICO_LOG_STATUS = "HOME/LOG/STATUS";
        const string TOPICO_GATEWAY_ENTRADA = "HOME/GATEWAY/ENTRADA";
        const string TOPICO_GATEWAY_SAIDA = "HOME/GATEWAY/SAIDA";
        const string TOPICO_DESODORIZACAO = "HOME/DESODORIZACAO";
        const string TOPICO_INTERFONE = "HOME/INTERFONE";
        const string TOPICO_PETS = "HOME/PETS";

        const string DEVICE_ID = "CONTROLLER_SMS";

        private readonly IMqttClientOptions Options;
        private readonly string menu = "Controller SMS API \r\nDigite a opcao abaixo: \r\nOP1 - Informar Temperatura \r\nOP2 - Desodorizar Ambiente \r\nOP3 - Abrir Portaria \r\nOP4 - Alimentar Pets";

        private IMqttClient client;

        public MosquittoMqttClient(IMqttClientOptions options)
        {
            Options = options;
            client = new MqttFactory().CreateMqttClient();
            client.UseApplicationMessageReceivedHandler(OnMessage);
        }

        public virtual async void OnMessage(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            try
            {
                var jsonPayload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
                var topic = eventArgs.ApplicationMessage.Topic;

                if (topic.Contains("HOME/GATEWAY/ENTRADA"))
                {
                    Console.WriteLine("Nova mensagem recebida do broker: ");
                    Console.WriteLine(jsonPayload);

                    var payload = JsonSerializer.Deserialize<PayloadMessage>(jsonPayload);

                    if (!string.IsNullOrEmpty(payload.message))
                    {
                        // procedimento para opção MENU ##########################################
                        if (payload.message.ToUpper().Equals("MENU"))
                        {
                            // informando o Menu
                            payload.message = menu;
                            jsonPayload = PrepareMsgToBroker<PayloadMessage>(payload);
                            await PublishMessageAsync(TOPICO_GATEWAY_SAIDA, jsonPayload);
                        }

                        // procedimento para opção OP1 - TEMPERATURA ##########################################
                        if (payload.message.ToUpper().Equals("OP1"))
                        {
                            Random rnd = new Random();
                            int temp = rnd.Next(10, 40);

                            // informando a execução da tarefa com a temp
                            payload.message = "Temperatura no momento: " + temp + " graus.";
                            jsonPayload = PrepareMsgToBroker<PayloadMessage>(payload);
                            await PublishMessageAsync(TOPICO_GATEWAY_SAIDA, jsonPayload);
                        }

                        // procedimento para opção OP2 - DESODORIZACAO ##########################################
                        if (payload.message.ToUpper().Equals("OP2"))
                        {
                            // enviando comando para o topico correspondente
                            payload.message = "ACT";
                            jsonPayload = PrepareMsgToBroker<PayloadMessage>(payload);
                            await PublishMessageAsync(TOPICO_DESODORIZACAO, jsonPayload);

                            // informando a execução da tarefa
                            payload.message = "Desodorizacao executada!";
                            jsonPayload = PrepareMsgToBroker<PayloadMessage>(payload);
                            await PublishMessageAsync(TOPICO_GATEWAY_SAIDA, jsonPayload);
                        }

                        // procedimento para opção OP3 - INTERFONE ##########################################
                        if (payload.message.ToUpper().Equals("OP3"))
                        {
                            // enviando comando para o topico correspondente
                            payload.message = "ACT";
                            jsonPayload = PrepareMsgToBroker<PayloadMessage>(payload);
                            await PublishMessageAsync(TOPICO_INTERFONE, jsonPayload);

                            // informando a execução da tarefa
                            payload.message = "Portaria aberta!";
                            jsonPayload = PrepareMsgToBroker<PayloadMessage>(payload);
                            await PublishMessageAsync(TOPICO_GATEWAY_SAIDA, jsonPayload);
                        }

                        // procedimento para opção OP4 - PETS ##########################################
                        if (payload.message.ToUpper().Equals("OP4"))
                        {
                            // enviando comando para o topico correspondente
                            payload.message = "ACT";
                            jsonPayload = PrepareMsgToBroker<PayloadMessage>(payload);
                            await PublishMessageAsync(TOPICO_PETS, jsonPayload);

                            // informando a execução da tarefa
                            payload.message = "Pets alimentados!";
                            jsonPayload = PrepareMsgToBroker<PayloadMessage>(payload);
                            await PublishMessageAsync(TOPICO_GATEWAY_SAIDA, jsonPayload);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao tentar ler a msnsagem: " + ex.Message);
                //throw;
            }
        }

        public string PrepareMsgToBroker<T>(object payload)
        {
            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                //WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return jsonPayload;
        }

        public async Task PublishMessageAsync(string topic, string payload, bool retainFlag = false, int qos = 0)
        {
            Console.WriteLine("Enviando mensagem para o broker: ");
            Console.WriteLine(payload);

            Console.Write("Topico: ");
            Console.WriteLine(topic);

            await client.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .WithRetainFlag(retainFlag)
                .Build());
        }

        public async Task StartClientAsync()
        {
            await client.ConnectAsync(Options);

            // anuncia status online
            PayloadStatus payload = new PayloadStatus();
            payload.device = DEVICE_ID;
            payload.status = "Online";
            payload.connectedOn = DateTime.Now.ToString();

            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(TOPICO_LOG_STATUS).Build());
            var jsonPayload = PrepareMsgToBroker<PayloadStatus>(payload);
            await PublishMessageAsync(TOPICO_LOG_STATUS, jsonPayload);

            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(TOPICO_GATEWAY_ENTRADA).Build());
            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(TOPICO_GATEWAY_SAIDA).Build());

            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(TOPICO_DESODORIZACAO).Build());
            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(TOPICO_INTERFONE).Build());
            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(TOPICO_PETS).Build());

            if (!client.IsConnected)
            {
                await client.ReconnectAsync();
            }
        }

        public Task StopClientAsync()
        {
            throw new NotImplementedException();
        }
    }
}
