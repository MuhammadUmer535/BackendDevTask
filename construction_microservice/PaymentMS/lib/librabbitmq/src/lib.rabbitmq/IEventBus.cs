using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Lib.RabbitMq.ServiceCommand;
using System.Xml.Linq;

namespace Lib.RabbitMq
{
    public interface IEventBus
    {
        void PublishRequest(ServiceRequest request, string queueName);
        void PublishRequest(XDocument request, string queueName);
        void PublishReply(ServiceReply reply, string replyQueueName, string correlationId);
        void Subscribe(string queueName, RabbitMqListener listener, IConfiguration configuration, IEventBus eventBus);
        ServiceReply Call(ServiceRequest request, string queueName, string replyQueueName, string correlationId);

       
    }
}