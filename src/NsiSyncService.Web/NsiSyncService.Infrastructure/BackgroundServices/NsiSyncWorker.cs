using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NsiSyncService.Core;
using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Infrastructure.BackgroundServices;

public class NsiSyncWorker : BackgroundService
{
    private readonly DbInitializer _dbInitializer;
    private readonly ILogger<NsiSyncWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    private readonly List<string> _referenceIdentifiers = new() 
    { 
        // тестовый список для проверки версий у этих документов
        "F017", "F018", "F019" 
    };
    
    public NsiSyncWorker(DbInitializer dbInitializer, IServiceProvider serviceProvider, ILogger<NsiSyncWorker> logger)
    {
        _dbInitializer = dbInitializer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting NsiSyncWorker");
        
        await _dbInitializer.InitializeAsync(cancellationToken);
        
        await base.StartAsync(cancellationToken);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Начало работы со внешним API");
                
                foreach (var identifier in _referenceIdentifiers)
                {
                    using var scope = _serviceProvider.CreateScope();
                    
                    var syncProvider = scope.ServiceProvider.GetRequiredService<ISyncProvider>();
                    await syncProvider.SyncReferenceAsync(identifier, stoppingToken);
                    
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception, что-то пошло не так");
            }
            
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}