using Lib.RabbitMq;
using Lib.RabbitMq.ServiceCommand;
using System.Text.Json;
using Lib.RabbitMq.RPC;

public class RPCService : RPCServer, IHostedService, IDisposable
{

    #region Hosted Service Implementation
    private Task _executingTask;
    private CancellationTokenSource _cts;
    private IConfiguration _configuration;

    public RPCService(IConfiguration configuration)
    {

        this._configuration = configuration;
        //_eventBus = eventBus;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = ExecuteAsync(cancellationToken);
        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {

        if (_executingTask == null)
        {
            return;
        }
        try
        {
            _cts.Cancel();
            this.Stop();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }
    protected Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => { this.Start(MessageQueues.PAYMENTRPCQueue); });
    }

    #endregion

    public override ServiceReply ProcessMessage(ServiceRequest svcRequest)
    {
        ServiceReply svcReply = null;

        svcReply = new ServiceReply()
        {
            data = string.Empty,
            code = "303",
            exception = string.Empty,
            fullDescription = string.Empty,
            message = "Received"
        };
        return svcReply;
    }

    public virtual void Dispose()
    {
        this.Stop();
    }
}