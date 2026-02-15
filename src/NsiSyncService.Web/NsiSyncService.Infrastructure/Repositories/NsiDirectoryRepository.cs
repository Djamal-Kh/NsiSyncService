using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Infrastructure.Repositories;

public class NsiDirectoryRepository : INsiDirectoryRepository
{
    public NsiDirectoryRepository(HttpClient httpClient)
    {
    }
    
    public Task GetVersionAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}