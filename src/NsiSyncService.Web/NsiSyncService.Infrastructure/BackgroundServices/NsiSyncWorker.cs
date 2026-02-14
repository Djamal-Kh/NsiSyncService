using Microsoft.Extensions.Hosting;

namespace NsiSyncService.Infrastructure.BackgroundServices;

public class NsiSyncWorker : BackgroundService
{
    private readonly DbInitializer _dbInitializer;
    
    public NsiSyncWorker(DbInitializer dbInitializer)
    {
        _dbInitializer = dbInitializer;
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting NsiSyncWorker...");
        await _dbInitializer.InitializeAsync(cancellationToken);
        
        await base.StartAsync(cancellationToken);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Проверить наличие новой версии на сайте
            // 2. Если есть -> Спарсить данные (Dapper)
            // 3. Запустить миграцию Actual -> History
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}