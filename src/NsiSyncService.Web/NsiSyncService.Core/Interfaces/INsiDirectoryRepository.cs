using NsiSyncService.Core.DTOs;
using NsiSyncService.Core.Entities;

namespace NsiSyncService.Core.Interfaces;

public interface INsiDirectoryRepository
{
    public Task<string?> GetLastVersionFromDbAsync(string identifier, CancellationToken cancellationToken);

    public Task InsertRecordToDbAsync(string identifier, DataDto dbData , CancellationToken cancellationToken);
    
    public Task RotateDirectoryDataAsync(string identifier, VersionInfoDto dbVersion, DataDto dbData , CancellationToken cancellationToken);

    public Task CreateTableAsync(string identifier, StructureDto dbStructure, CancellationToken cancellationToken);

    public Task UpdateTablesAsync(StructureDto dbStructure, CancellationToken cancellationToken);
}