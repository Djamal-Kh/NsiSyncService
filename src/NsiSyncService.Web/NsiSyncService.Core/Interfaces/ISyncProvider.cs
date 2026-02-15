namespace NsiSyncService.Core;

public interface ISyncProvider
{
    public Task SyncReferenceAsync(string identifier, CancellationToken cancellationToken = default);
}