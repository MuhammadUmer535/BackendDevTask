using System;
using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using Lib.RabbitMq.ServiceCommand;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Lib.RabbitMq
{
    public class RabbitMqBus : IEventBus
    {
        private IConnectionFactory _factory;
        private const int RETRY_DELAY = 300000;
        private readonly IHostApplicationLifetime _lifetime;
        private IConfiguration _configuration;

        public RabbitMqBus(IHostApplicationLifetime lifetime, IConfiguration configuration)
        {
            _lifetime = lifetime;
            _configuration = configuration;
            var rabbitMqConfig = _configuration.GetSection("RabbitMqConfig");
            _factory = new ConnectionFactory()
            {
                HostName = rabbitMqConfig["HostName"]?.ToString(),
                UserName = rabbitMqConfig["UserName"]?.ToString(),
                Password = rabbitMqConfig["Password"]?.ToString(),
                Port = Int32.Parse(rabbitMqConfig["Port"]),
                VirtualHost = rabbitMqConfig["VirtualHost"]?.ToString()
            };
        }

        public void PublishReply(ServiceReply reply, string replyQueueName, string correlationId)
        {
            try
            {
                var replyBytes = RabbitMqHelper.ObjectToByteArray(reply);
                using (var connection = _factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        var basicProps = channel.CreateBasicProperties();
                        basicProps.CorrelationId = correlationId;
                        channel.QueueDeclare(replyQueueName, durable: true, false, false, null);

                        channel.BasicPublish(
                            exchange: "",
                            routingKey: replyQueueName,
                            body: replyBytes,
                            basicProperties: basicProps);
                        Console.WriteLine("Message Published on queue: {0}", replyQueueName);
                    }
                }
            }

            //TODO : handle connection exception.
            catch (Exception exception)
            {
                return;
            }
        }

        public void PublishRequest(ServiceRequest request, string queueName)
        {
            try
            {
                var requestBytes = RabbitMqHelper.ObjectToByteArray(request);
                using (var connection = _factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queueName, durable: true, false, false, null);

                        channel.BasicPublish(
                            exchange: "",
                            routingKey: queueName,
                            body: requestBytes);
                        Console.WriteLine("Message Published on queue: {0}", queueName);
                    }
                }
            }

            //TODO : handle connection exception.
            catch (Exception exception)
            {
                return;
            }
        }
        public void PublishRequest(XDocument request, string queueName)
        {
            try
            {
                var requestBytes = RabbitMqHelper.XDocumentToByte(request);

                using (var connection = _factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queueName, durable: true, false, false, null);

                        channel.BasicPublish(
                            exchange: "",
                            routingKey: queueName,
                            body: requestBytes);
                        Console.WriteLine("Message Published on queue: {0}", queueName);
                    }
                }
            }

            //TODO : handle connection exception.
            catch (Exception ex)
            {
                return;
            }
        }
        public void Subscribe(string queueName, RabbitMqListener listener, IConfiguration configuration, IEventBus eventBus)
        {
            _lifetime.ApplicationStarted.Register(() =>
            {
                listener.SubscribeProcess(queueName, configuration, eventBus);
            });
        }
        public ServiceReply Call(ServiceRequest request, string queueName, string replyQueueName, string correlationId)
        {
            try
            {
                var requestBytes = RabbitMqHelper.ObjectToByteArray(request);

                using (var connection = _factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queueName, durable: true, false, false, null);
                        channel.QueueDeclare(replyQueueName, durable: true, false, false, null);
                        var respQueue = new BlockingCollection<byte[]>();
                        var consumer = new EventingBasicConsumer(channel);

                        var props = channel.CreateBasicProperties();
                        props.CorrelationId = correlationId;
                        props.ReplyTo = replyQueueName;

                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body.ToArray();

                            if (ea.BasicProperties.CorrelationId == correlationId)
                            {
                                respQueue.Add(body);
                            }
                        };

                        channel.BasicPublish(
                            exchange: "",
                            routingKey: queueName,
                            basicProperties: props,
                            body: requestBytes);

                        channel.BasicConsume(
                            consumer: consumer,
                            queue: replyQueueName,
                            autoAck: true);

                        var responseBytes = respQueue.Take();
                        return (ServiceReply)RabbitMqHelper.ByteArrayToObject(responseBytes);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}