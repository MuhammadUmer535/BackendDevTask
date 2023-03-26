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
    // rpcClient.Close();
    /// <summary>
    ///  RPC client to consume and send request to RPC server
    /// </summary>
    public class RPCClient
    {
        #region  Field
        private string QUEUE_NAME = "";
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> callbackMapper =
                    new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();

        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHostName">RabbitMQ Server's IP</param>
        /// <param name="pQueueName">Default Queue name to send message</param>
        public RPCClient(string pQueueName)
        {
            this.QUEUE_NAME = pQueueName;
            connection = ConnectionList.Instance.GetWriteConnection();
            channel = connection.CreateModel();
            // declare a server-named queue
            replyQueueName = channel.QueueDeclare(queue: "").QueueName;  //Queue Name is randomly generated for this RPC Client
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<byte[]> tcs))
                    return;
                var body = ea.Body.ToArray();
                //var response = Encoding.UTF8.GetString(body);
                tcs.TrySetResult(body);
            };

            channel.BasicConsume(
            consumer: consumer,
            queue: replyQueueName,
            autoAck: false);
        }
        #endregion

        #region Public Methods

        /// <summary>
        ///     CallAsync
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> CallAsync(ServiceRequest message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.CallAsync(message, this.QUEUE_NAME, cancellationToken);
        }
        /// <summary>
        ///     CallAsync
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> CallAsync(ServiceRequest message, string queue, CancellationToken cancellationToken = default(CancellationToken))
        {
            IBasicProperties props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;
            var messageBytes = RabbitMqHelper.ObjectToByteArray(message);
            var tcs = new TaskCompletionSource<byte[]>();
            callbackMapper.TryAdd(correlationId, tcs);

            channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: props,
                body: messageBytes);

            cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
            return tcs.Task;
        }

        public void Close()
        {
            if (channel != null && channel.IsOpen == true)
                channel.Close();
        }

        ~RPCClient()
        {
            if (channel != null && channel.IsOpen == true)
                channel.Close();
        }

        #endregion
    }
}