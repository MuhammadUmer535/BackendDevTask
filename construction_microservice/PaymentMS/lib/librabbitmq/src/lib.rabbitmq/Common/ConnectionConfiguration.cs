using System;
using Microsoft.Extensions.Configuration;

namespace Lib.RabbitMq
{
    public class ConnectionConfiguration
    {
        public string HostName{get;set;}
        public string UserName{get;set;}
        public string Password { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }

        public ConnectionConfiguration()
        {
            
        }


        public static ConnectionConfiguration Load()
        {
            string _hostName="localhost";
            string _userName="guest";
            string _password="guest";
            int    _port=5672;
            string _virtualHost="/";

            IConfigurationBuilder builder = new ConfigurationBuilder();
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            builder.AddJsonFile("appsettings.json", optional:true, reloadOnChange:false);
            if(!string.IsNullOrWhiteSpace(env)){
                builder.AddJsonFile(string.Format("appsettings.{0}.json",env), optional:true, reloadOnChange:false);
            }
            IConfigurationSection section = builder.Build()?.GetSection("RabbitMqConfig");

        
            string strValue=section["HostName"]?.ToString().Trim();
            if(!string.IsNullOrWhiteSpace(strValue))
            {
                _hostName=strValue;
            }

            strValue=section["UserName"]?.ToString().Trim();
            if(!string.IsNullOrWhiteSpace(strValue))
            {
                _userName=strValue;
            }

            strValue=section["Password"]?.ToString().Trim();
            if(!string.IsNullOrWhiteSpace(strValue))
            {
                _password=strValue;
            }

            strValue=section["Port"]?.ToString().Trim();
            if(!string.IsNullOrWhiteSpace(strValue))
            {
                int port=0;
                if(Int32.TryParse(strValue, out port))
                    _port=port;
            }

             strValue=section["VirtualHost"]?.ToString().Trim();
            if(!string.IsNullOrWhiteSpace(strValue))
            {
                _virtualHost=strValue;
            }

            return new ConnectionConfiguration(){
                HostName=_hostName,
                UserName=_userName,
                Password=_password,
                Port=_port,
                VirtualHost=_virtualHost
            };
        }
    }
}