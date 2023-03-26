using System;
using Microsoft.Extensions.Configuration;

namespace Lib.RabbitMq
{
    public class ConfigurationFactory
    {
        private string _hostname;
        private string _username;
        private string _password;
        private int? _port;
        private string _virtualHost;
        private int? _requestedHeartbeat;
        private bool? _automaticRecoveryEnabled;

        public string Hostname
        {
            get
            {
                return _hostname;
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
        }

        public int? Port
        {
            get
            {
                return _port;
            }
        }

        public string VirtualHost
        {
            get
            {
                return _virtualHost;
            }
        }

        public int? RequestedHeartbeat
        {
            get
            {
                return _requestedHeartbeat;
            }
        }
        public bool? AutomaticRecoveryEnabled
        {
            get
            {
                return _automaticRecoveryEnabled;
            }
        }

        public ConfigurationFactory(IConfigurationSection ConfigSection)
        {
            _hostname = ConfigSection["HostName"]?.ToString();
            _username = ConfigSection["UserName"]?.ToString();
            _password = ConfigSection["Password"]?.ToString();
            _virtualHost = ConfigSection["VirtualHost"]?.ToString();

            var portConfigvalue = ConfigSection["Port"]?.ToString();
            var heartbeatConfigValue = ConfigSection["RequestedHeartbeat"]?.ToString();
            var automaticRecoveryEnabledConfigValue = ConfigSection["AutomaticRecoveryEnabled"]?.ToString().ToLower();

            if(portConfigvalue != null && Int32.TryParse(portConfigvalue, out int providedPort))
            {
                _port = providedPort;
            }

            if(heartbeatConfigValue != null && Int32.TryParse(heartbeatConfigValue, out int providedHeartbeat))
            {
                _requestedHeartbeat = providedHeartbeat;
            }

            if(automaticRecoveryEnabledConfigValue != null && bool.TryParse(automaticRecoveryEnabledConfigValue, out bool providedAutoRecVal))
            {
                _automaticRecoveryEnabled = providedAutoRecVal;
            }
        }
    }
}