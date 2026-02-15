using NsiSyncService.Core.DTOs;
using NsiSyncService.Core.Entities;

namespace NsiSyncService.Core.Interfaces;

public interface INsiDirectoryRepository
{
    public Task<string?> GetLastVersionFromDbAsync(string identifier, CancellationToken cancellationToken);

    public Task InsertRecordToDb(string identifier, VersionInfoDto dto, CancellationToken cancellationToken);
    
    public Task RotateDirectoryDataAsync(string identifier, VersionInfoDto dto, CancellationToken cancellationToken);
}