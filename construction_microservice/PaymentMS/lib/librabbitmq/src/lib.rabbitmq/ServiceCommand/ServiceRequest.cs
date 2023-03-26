using System;

namespace Lib.RabbitMq.ServiceCommand
{
    [Serializable]
    public class ServiceRequest
    {
        public string data { get; set; }
        public string methodId { get; set; }
        public string WFEvent { get; set; }     
    }
}