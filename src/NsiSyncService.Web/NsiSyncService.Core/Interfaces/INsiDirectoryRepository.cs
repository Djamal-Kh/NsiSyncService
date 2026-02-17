using System.Data;
using NsiSyncService.Core.DTOs;
using NsiSyncService.Core.Entities;

namespace NsiSyncService.Core.Interfaces;

public interface INsiDirectoryRepository
{
    public Task<string?> GetLastVersionFromDbAsync(string identifier, CancellationToken cancellationToken);

    public Task InsertRecordToDbAsync(
        string identifier, 
        DataDto dbData, 
        StructureDto dbStructure, 
        CancellationToken cancellationToken = default, 
        IDbConnection connection = null, 
        IDbTransaction transaction = null);
    
    public Task RotateDirectoryDataAsync(
        string identifier,
        VersionInfoDto apiVersion, 
        string currentVersion, 
        StructureDto dbStructure, 
        DataDto dbData, 
        CancellationToken cancellationToken);

    public Task CreateTablesAsync(string identifier, StructureDto dbStructure, CancellationToken cancellationToken);

    public Task AddVersionAsync(string identifier, VersionInfoDto dbVersion, CancellationToken cancellationToken);
}