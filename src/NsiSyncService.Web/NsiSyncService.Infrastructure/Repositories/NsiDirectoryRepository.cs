using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using NsiSyncService.Core.DTOs;
using NsiSyncService.Core.Entities;
using NsiSyncService.Core.Extensions;
using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Infrastructure.Repositories;

public class NsiDirectoryRepository : INsiDirectoryRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<NsiDirectoryRepository> _logger;

    public NsiDirectoryRepository(IDbConnectionFactory dbConnectionFactory, ILogger<NsiDirectoryRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<string?> GetLastVersionFromDbAsync(string identifier, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql =
            """
            SELECT CurrentVersion
            FROM dbo.Directory_Actual_Version  
            WHERE Code = @DirectoryCode
            """;

        var param = new { DirectoryCode = identifier };
        
        var record = await connection.QuerySingleOrDefaultAsync<string?>(sql, param);
        
        return record;
    }

    public async Task InsertRecordToDbAsync(string identifier, DataDto dbData, StructureDto dbStructure, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        
        var tableName = $"{identifier}_Actual";
        
        var existingInDb = dbStructure.Columns.Select(c => c.Name).ToList();
        existingInDb.Add("SYS_RECORDID");
        existingInDb.Add("SYS_HASH");
        
        // убираем столбцы, которых нет в structure, но есть в data
        var columns = dbData.List.First()
            .Select(x => x.Column)
            .Where(c => existingInDb.Contains(c))
            .ToList();
        
        var columnsSql = SqlMappingExtensions.ToSqlColumnsForInsert(columns);
        
        // переделать под List ?
        // также здесь проходит фильтрация
        var values = dbData.List.Select(row => 
            row.Where(cell => columns.Contains(cell.Column))
                .Select(x => x.Value));
        
        var valuesSql = SqlMappingExtensions.ToSqlValuesForInsert(values);
        
        string sql = @$"
        INSERT INTO dbo.{tableName} ({columnsSql})
        VALUES {valuesSql}";
        
        try
        {
            // вместо Dapper тут использую ADO.NET т.к. с Dapper`ом начинается проблема из-за символа @
            await connection.ExecuteAsync(sql, transaction: transaction);
            
            transaction.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inserting record to database");
            transaction.Rollback();
            throw;
        }
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error inserting record to database");
            transaction.Rollback();
            throw;
        }
    }

    public async Task CreateTablesAsync(string identifier, StructureDto dbStructure, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        string actualTable = $"{identifier}_Actual";
        string historyTable = $"{identifier}_History";
        
        // переменная для добавления в таблицу динамического количества столбец
        var columnDefinitions = dbStructure.Columns.Select(c =>
        {
            string sqlType = SqlMappingExtensions.ToSqlType(c);
            //string nullabillity = c.EmptyAllowed ? "NULL" : "NOT NULL"; - вынужденно т.к. даже если указано NOT NULL - приходили пустые значения
            string nullabillity = "NULL";
            return $"[{c.Name}] {sqlType} {nullabillity}";
        });
        
        string allColumnsSql = string.Join(",", columnDefinitions);
        
        // Жесткий костыль, небезопасно так писать
        string sql = $@"
        IF OBJECT_ID('{actualTable}') IS NULL
        CREATE TABLE {actualTable}
        (
            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            [SYS_RECORDID] NVARCHAR(255) NULL, 
            [SYS_HASH] NVARCHAR(255) NULL,
            {allColumnsSql}
        );

        IF OBJECT_ID('{historyTable}') IS NULL
        CREATE TABLE [{historyTable}]
        (
            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            {allColumnsSql},
            [VersionId] NVARCHAR(50) NOT NULL,
            [Archived_Date] DATETIME NOT NULL
        );";
        
        try
        {
            await connection.ExecuteAsync(sql, transaction: transaction);

            transaction.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating tables");
            transaction.Rollback();
            throw;
        }
    }

    public async Task AddVersionAsync(string identifier, VersionInfoDto dbVersion, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql =
            """
            INSERT INTO dbo.Directory_Actual_Version (Code, CurrentVersion, LastUpdate)
            VALUES (@DirectoryCode, @Version, @LastUpdate)
            """;
        
        var param = new
        {
            DirectoryCode = identifier,
            Version = dbVersion.Version,
            LastUpdate = dbVersion.LastUpdate
        };
        
        await connection.ExecuteAsync(sql, param);
    }
}
