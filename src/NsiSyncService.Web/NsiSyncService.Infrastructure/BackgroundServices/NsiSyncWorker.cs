using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NsiSyncService.Core;
using NsiSyncService.Core.Extensions.ExceptionExtensions;
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
        "F001", "V022" , "F000" , "F002", "F003", "F004", "V042", "F005", "N015", "V017"
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
            foreach (var identifier in _referenceIdentifiers)
            {
                try
                {
                    _logger.LogInformation("Work with Api. Processed identifier: {identifier}", identifier);
                    using var scope = _serviceProvider.CreateScope();

                    var syncProvider = scope.ServiceProvider.GetRequiredService<ISyncProvider>();
                    await syncProvider.SyncReferenceAsync(identifier, stoppingToken);

                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }

                catch (ResourceNotFoundException)
                {
                    _logger.LogError("Failed to retrieve data from external API. Identifier: {Identifier}", identifier);
                }
                    
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Unhandled exception");
                    throw;
                }
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}