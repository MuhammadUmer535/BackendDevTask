using Lib.RabbitMq.ServiceCommand;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.RabbitMq.RPC
{
    /// <summary>
    /// RPC Server to communicate sync calls
    /// </summary>

    public class RPCServer
    {
        #region Fields
        private string QUEUE_NAME = "";
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> callbackMapper =
                    new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();
        private bool cancelationPending = false;

        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public RPCServer()
        {

        }

        #endregion

        #region  Public Method
        /// <summary>
        ///     ProcessMessage
        /// </summary>
        /// <param name="svcRequest"></param>
        /// <returns></returns>
        public virtual ServiceReply ProcessMessage(ServiceRequest svcRequest)
        {
            return new ServiceReply();
        }

        /// <summary>
        ///     Start
        /// </summary>
        /// <param name="pQueueName"></param>
        public void Start(string pQueueName)
        {

            this.QUEUE_NAME = pQueueName;
            cancelationPending = false;
            IConnection connection = ConnectionList.Instance.GetReadConnection();
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: this.QUEUE_NAME, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: this.QUEUE_NAME, autoAck: false, consumer: consumer);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;
                    ServiceReply svcReply = null; //TODO: null response??
                    try
                    {
                        var correlationId = props.CorrelationId;
                        var replyTo = props.ReplyTo;
                        var svcRequest = (ServiceRequest)RabbitMqHelper.ByteArrayToObject(body);
                        //svcReply = ProcessMessage(svcRequest);
                        svcReply = ProcessMessage(svcRequest);

                    }
                    catch (Exception e)
                    {
                        svcReply = new ServiceReply()
                        {
                            code = "501",
                            exception = e.Message,
                            message = "Exception has occured."
                        };
                    }
                    finally
                    {
                        var responseBytes = RabbitMqHelper.ObjectToByteArray(svcReply);// 
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };

                while (!cancelationPending)
                {
                    Thread.Sleep(1000);
                }
            }

            cancelationPending = false;
        }


        /// <summary>
        ///     Stop
        /// </summary>
        public void Stop()
        {
            this.cancelationPending = true;
        }

        /// <summary>
        ///     Close
        /// </summary>        
        public void Close()
        {

        }

        ~RPCServer()
        {

        }

        #endregion
    }
}