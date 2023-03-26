using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Lib.RabbitMq.ServiceCommand;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;

namespace Lib.RabbitMq
{
    public class RabbitMqListener : IListener
    {
        private IConnectionFactory _factory;
        private EventingBasicConsumer _consumer;
        private IModel _channel;
        private string _queueName;
        private IConfiguration _configuration;
        private int DefaultHeartbeat = 60;
        private bool DefaultAutoRecEnabledVal = true;

        public RabbitMqListener(IConfiguration configuration)
        {
            _configuration = configuration;
            var rabbitMqConfig = _configuration.GetSection("RabbitMqConfig");
            var configFactory = new ConfigurationFactory(rabbitMqConfig);
            var heartbeat = configFactory.RequestedHeartbeat ?? DefaultHeartbeat;
            var autoRecEnabled = configFactory.AutomaticRecoveryEnabled ?? DefaultAutoRecEnabledVal;
            _factory = new ConnectionFactory()
            {
                HostName = configFactory.Hostname,
                UserName = configFactory.Username,
                Password = configFactory.Password,
                Port = configFactory.Port ?? 0,
                VirtualHost = configFactory.VirtualHost,
                RequestedHeartbeat = TimeSpan.FromSeconds(heartbeat),
                AutomaticRecoveryEnabled = autoRecEnabled
            };
        }

        public void CreateConsumer()
        {
            try
            {
                _channel = _factory.CreateConnection().CreateModel();
                var exchange = GetQueueExchangeType(_queueName);
                _channel.ExchangeDeclare(exchange, "direct");
                _channel.QueueDeclare(_queueName, durable: true, false, false, null);
                _channel.QueueBind(_queueName, exchange, string.Empty, null);
                _consumer = new EventingBasicConsumer(_channel);
            }

            catch (Exception exception)
            {
                return;
            }
        }

        public void Consume()
        {
            _channel.BasicConsume(_queueName, true, _consumer);
        }

        public void SubscribeProcess(string queueName, IConfiguration configuration, IEventBus eventBus)
        {
            try
            {
                _queueName = queueName;
                CreateConsumer();

                _consumer.Received += ((s, e) =>
                {
                    var body = e.Body.ToArray();
                    var correlationId = e.BasicProperties.CorrelationId;
                    var replyTo = e.BasicProperties.ReplyTo;
                    var svcRequest = (ServiceRequest)RabbitMqHelper.ByteArrayToObject(body);
                    ProcessMessage(svcRequest, configuration, eventBus, correlationId, replyTo);
                    var mess = Encoding.UTF8.GetString(body);
                });

                Consume();
            }

            catch (Exception exception)
            {
                return;
            }
        }

        public virtual void ProcessMessage(ServiceRequest request, IConfiguration configuration, IEventBus eventBus, string correlationId, string replyTo)
        {
        }

        private string GetQueueExchangeType(string queueName)
        {
            var exchange = string.Empty;
            if (string.IsNullOrWhiteSpace(queueName)) return null;
            exchange = string.Concat(queueName, "_Exchange");
            return exchange;
        }
    }
}
