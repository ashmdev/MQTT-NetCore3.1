using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Server;
namespace MQTTDaemon
{
    public class MqttService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IOptions<MQTTConfig> _config;
        private IMqttServer _mqttServer;
        public MqttService(ILogger<MqttService> logger, IOptions<MQTTConfig> config)
        {
            _logger = logger;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT Daemon on port " + _config.Value.Port);

            // OK

            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionBacklog(1000)
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(_config.Value.Port);
                //.WithConnectionValidator(OnNewConnection)
                //.WithApplicationMessageInterceptor(OnNewMessage);


            //Getting an MQTT Instance/creates a new mqtt server   
            _mqttServer = new MqttFactory().CreateMqttServer();
            
            //Wiring up all the events...

            _mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(e =>
            {
                _logger.LogInformation(e.ClientId + " subscribed to " + e.TopicFilter);
            });
            
            _mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(e =>
            {
                _logger.LogInformation(e.ClientId + " unsubscribed to " + e.TopicFilter);
            });
            _mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e =>
            {
                _logger.LogInformation(e.ClientId + " Connected.");
            });
            _mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e =>
            {
                _logger.LogInformation(e.ClientId + " Disonnected.");
            });
            _mqttServer.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
            {
                _logger.LogInformation(e.ClientId + " published message to topic " + e.ApplicationMessage.Topic);
            });

   
            
            return _mqttServer.StartAsync(optionsBuilder.Build());
        }




        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT Daemon.");
            return _mqttServer.StopAsync();
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing....");
        }
    }
    
}