using Microsoft.Extensions.Configuration;
using Lib.RabbitMq.ServiceCommand;

namespace Lib.RabbitMq
{
    public interface IListener
    {
        void CreateConsumer();
        void SubscribeProcess(string queueName, IConfiguration configuration, IEventBus eventBus);
        void Consume();
        void ProcessMessage(ServiceRequest request, IConfiguration configuration, IEventBus eventBus, string correlationId, string replyTo);
    }
}