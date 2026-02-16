using System.Text.Json;
using Dapper;
using NsiSyncService.Core.DTOs;
using NsiSyncService.Core.Entities;
using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Infrastructure.Repositories;

public class NsiDirectoryRepository : INsiDirectoryRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public NsiDirectoryRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<string?> GetLastVersionFromDbAsync(string identifier, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql =
            """
            SELECT CurrentVersion
            FROM dbo.Directory_Actual  
            WHERE Code = @DirectoryCode
            """;

        var param = new { DirectoryCode = identifier };
        
        var record = await connection.QuerySingleOrDefaultAsync<string?>(sql, param);
        
        return record;
    }

    public async Task InsertRecordToDbAsync(string identifier, DataDto dbData, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        /*
        const string sql =
            """
            Insert Into dbo.Directory_Actual (Code, Name, CurrentVersion, JsonData, LastUpdate) 
            VALUES (@DirectoryCode, @DirectoryName, @CurrentVersion, @JsonData, @LastUpdate)
            """;
        
        var jsonData = JsonSerializer.Serialize(dbStructure.Passport);
        
        var param = new
        {
            DirectoryCode = identifier,
            DirectoryName = dto.Passport.Name,
            CurrentVersion = dto.Version,
            JsonData = jsonData,
            LastUpdate = dto.LastUpdate
        };
        
        await connection.ExecuteAsync(sql, param);
        */
    }

    public async Task RotateDirectoryDataAsync(string identifier, VersionInfoDto dto, DataDto dbData, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        const string sql =
            """
            INSERT INTO dbo.Directory_History (Code, Name, Version, JsonData, ArchivedAt)
            SELECT Code, Name, CurrentVersion, JsonData, GETDATE()
            FROM dbo.Directory_Actual WITH (UPDLOCK, HOLDLOCK, ROWLOCK)
            WHERE Code = @DirectoryCode
            
            UPDATE dbo.Directory_Actual
            SET Name = @DirectoryName, 
                CurrentVersion = @DirectoryVersion, 
                JsonData = @JsonData, 
                LastUpdate = @LastUpdate
            WHERE Code = @DirectoryCode
            """;

        var jsonData = JsonSerializer.Serialize(dto.Passport);

        var param = new
        {
            DirectoryCode = identifier,
            DirectoryName = dto.Passport.Name,
            DirectoryVersion = dto.Version,
            JsonData = jsonData,
            LastUpdate = dto.LastUpdate
        };

        try
        {
            await connection.ExecuteAsync(sql, param, transaction);

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task CreateTableAsync(string identifier, StructureDto dbStructure, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateTablesAsync(StructureDto dbStructure, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}