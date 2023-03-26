using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Lib.RabbitMq
{

    // HOW TO CALL RPC CLient
    // var rpcClient = new RpcClient("localhost");

    // Console.WriteLine(" [x] Requesting fib({0})", n);
    // var response = await rpcClient.CallAsync(n.ToString());
    // Console.WriteLine(" [.] Got '{0}'", response);

    // rpcClient.Close();

    public class ConnectionList
    {
              

        #region  Singleton Implementation
        private static object _instanceLock=new  Object();
        private static ConnectionList _instance=null;

        public static ConnectionList Instance{
            get{

                if(_instance==null)
                {
                    lock(_instanceLock)
                    {
                        if(_instance==null)
                         {  
                              _instance=new ConnectionList();
                         }
                    }
                }
                return _instance;
            }
        }
        protected ConnectionList(){
            Initialize();
        }
        #endregion

        private ConnectionConfiguration  _ConnectionConfiguration;
        private IConnection _WriteConnection;
        private IConnection _ReadConnection;
        protected void Initialize(){
            Console.WriteLine("Initialized");
            _ConnectionConfiguration=ConnectionConfiguration.Load();
            ConnectionFactory factory = new ConnectionFactory() { 
                HostName = _ConnectionConfiguration.HostName,
                //TODO: Should following be parameterized?
                UserName = _ConnectionConfiguration.UserName,
                Password = _ConnectionConfiguration.Password,
                Port = _ConnectionConfiguration.Port,
                VirtualHost = _ConnectionConfiguration.VirtualHost 
            };
            // Log.Information("| Class Name : {ClassName} | Method Name : {Method}   | Starting Method ", this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            

            _WriteConnection = factory.CreateConnection();
            _ReadConnection = factory.CreateConnection();
        }

        public void Init()
        {
            //Empty init , do call it at startup
        }
        public IConnection GetWriteConnection()
        {
            return _WriteConnection;
        }


        public IConnection GetReadConnection()
        {
            return _ReadConnection;
        }


        ~ConnectionList(){
            this.Dispose();
        }

        public void Dispose()
        {
            if(_ReadConnection!=null && _ReadConnection.IsOpen)
                _ReadConnection.Close();

            if(_WriteConnection!=null && _WriteConnection.IsOpen)
                _WriteConnection.Close();
        }
    }
}