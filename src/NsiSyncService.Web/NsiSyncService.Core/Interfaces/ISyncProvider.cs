namespace NsiSyncService.Core.Interfaces;

public interface ISyncProvider
{
    public Task SyncReferenceAsync(string identifier, CancellationToken cancellationToken = default);
}