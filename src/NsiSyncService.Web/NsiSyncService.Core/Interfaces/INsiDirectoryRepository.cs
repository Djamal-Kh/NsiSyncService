namespace NsiSyncService.Core.Interfaces;

public interface INsiDirectoryRepository
{
    public Task GetVersionAsync(CancellationToken cancellationToken);
}