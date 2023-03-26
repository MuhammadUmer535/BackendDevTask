using System;

namespace Lib.RabbitMq.ServiceCommand
{
    [Serializable]
    public class ServiceReply
    {
        public string data { get; set; }
        public string exception { set; get; }
        public string shortDescription { get; set; }
        public string fullDescription { get; set; }
        public string code { get; set; }
        public string message { set; get; }
    }
}