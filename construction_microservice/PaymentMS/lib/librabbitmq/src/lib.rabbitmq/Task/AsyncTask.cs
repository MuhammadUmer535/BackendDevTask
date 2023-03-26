using System;
using System.Collections.Generic;
using System.Reflection;
using Lib.RabbitMq.ServiceCommand;
using RabbitMQ.Client;

namespace Lib.RabbitMq.Task
{
    /// <summary>
    ///   AsyncTask class will be used for making async calls to RabbitMQ 
    /// </summary>
    public class AsyncTask
    {
        #region Field
        private string QUEUE_NAME = "";
        private readonly IConnection connection;
        private readonly IModel channel;

        #endregion

        #region  Constructor 
        public AsyncTask(string pQueueName)
        {
            this.QUEUE_NAME = pQueueName;
            connection = ConnectionList.Instance.GetWriteConnection();
            channel = connection.CreateModel();
            // declare a server-named queue
            channel.QueueDeclare(queue: this.QUEUE_NAME, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        #endregion

        #region Public methods 

        /// <summary>
        ///     Execute
        /// </summary>
        /// <param name="svcRequest"></param>
        public void Execute(ServiceRequest svcRequest)
        {
            try 
            {
                var body = RabbitMqHelper.ObjectToByteArray(svcRequest);

                var properties = channel.CreateBasicProperties();
                var header = new Dictionary<string, object>();
                header.Add("max-retry-attempt", (long) 10);
                //header.Add("retry-attempt", 0);
                properties.Persistent = true;
                properties.CorrelationId = Guid.NewGuid().ToString();
                properties.Headers = header;
                channel.BasicPublish(exchange: "", routingKey: this.QUEUE_NAME, basicProperties: properties, body: body);     
            }
            catch (Exception exception) 
            {
            }          
        }

        public void Close()
        {
            if (channel != null && channel.IsOpen == true)
                channel.Close();
        }

        ~AsyncTask()
        {
            if (channel != null && channel.IsOpen == true)
                channel.Close();
        }

        #endregion
    }
}