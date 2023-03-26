using Lib.RabbitMq.ServiceCommand;
using Lib.RabbitMq.Task;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.RabbitMq.Task
{
    public class AsyncWorker
    {
        #region Field
        private string QUEUE_NAME = "";
        private bool cancelationPending = false;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> callbackMapper =
                         new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();
        private Dictionary<string, long> RetryCounts = new Dictionary<string, long>();
        #endregion

        #region Constructor
        public AsyncWorker()
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
                channel.QueueDeclare(queue: this.QUEUE_NAME, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: this.QUEUE_NAME, autoAck: false, consumer: consumer);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var props = ea.BasicProperties;
                    try
                    {
                        ConsumeAsync(ea, channel);
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        if (GetRetryCount(ea.BasicProperties) > 0)
                        {
                            channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (GetRetryCount(ea.BasicProperties) > 0)
                        {
                            channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
                        }
                    }
                    finally
                    {

                    }
                };

                while (!cancelationPending)
                {
                    Thread.Sleep(1000);
                }
            }

            cancelationPending = false;
        }
        public Task<byte[]> ConsumeAsync(BasicDeliverEventArgs ea, IModel channel, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<byte[]>();
            try 
            {
                ServiceReply svcReply = null;
                var body = ea.Body.ToArray();
                var properties = ea.BasicProperties;
                tcs = new TaskCompletionSource<byte[]>(body);
                if (!RetryCounts.ContainsKey(properties.CorrelationId))
                {
                    RetryCounts.Add(properties.CorrelationId, 0);
                }
                callbackMapper.TryAdd(properties.CorrelationId, tcs);
                var svcRequest = (ServiceRequest)RabbitMqHelper.ByteArrayToObject(body);
                svcReply = ProcessMessage(svcRequest);
                if ((svcReply.code == "500" || svcReply.code == "401") && GetRetryCount(properties) > 0)
                {
                    channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
                }
                else
                {
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    RetryCounts.Remove(properties.CorrelationId);
                }

                cancellationToken.Register(() => callbackMapper.TryRemove(properties.CorrelationId, out var tmp));
                
            } 
            catch(Exception exception) 
            {
            }     

            return tcs.Task;       
        }

        /// <summary>
        ///     GetRetryCount
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        private long GetRetryCount(IBasicProperties properties)
        {
            long attempts = 0;
            if (!properties.Headers.ContainsKey("max-retry-attempt")) return attempts;

            var maxAttempts = (long)properties.Headers["max-retry-attempt"];
            RetryCounts.TryGetValue(properties.CorrelationId, out long lastRetryCount);
            if (lastRetryCount == maxAttempts) return attempts;

            lastRetryCount++;
            RetryCounts[properties.CorrelationId] = lastRetryCount;
            return maxAttempts - lastRetryCount;
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
            this.Stop();
        }
        ~AsyncWorker()
        {
            this.Close();
        }

        #endregion
    }
}