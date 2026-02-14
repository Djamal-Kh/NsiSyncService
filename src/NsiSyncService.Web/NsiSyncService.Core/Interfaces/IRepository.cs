namespace NsiSyncService.Core.Interfaces;

public interface IRepository
{
    Task<int> ExecuteAsync(string sql, CancellationToken cancellationToken);
}