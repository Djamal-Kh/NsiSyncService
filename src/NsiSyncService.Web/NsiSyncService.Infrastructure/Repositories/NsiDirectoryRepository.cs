using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using NsiSyncService.Core.DTOs;
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

    // Этот метод вызывается как напрямую из SyncProvider, так и из метода репозитория RotateDirectoryDataAsync
    // Поэтому здесь проверяется откуда был именно совершен вызов. Если напрямую из SyncProvider,
    // создать транзакцию и соединение с БД, если же из метода репозитория,
    // то тогда необходимо использовать "внешнюю" транзакцию
    public async Task InsertRecordsToDbAsync(
        string identifier, 
        DataDto dbData, 
        StructureDto dbStructure, 
        CancellationToken cancellationToken = default, 
        IDbConnection externalConnection = null, 
        IDbTransaction externalTransaction = null)
    {
        IDbConnection connection = externalConnection;
        
        // Если вызов метода напрямую из SyncProvider, то создаем соединение с БД
        if (connection is null)
            connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        try
        {
            // Если метод вызывается из SyncProvider, то тогда переменная isInternalTransaction имеет значение false
            bool isInternalTransaction = externalTransaction == null;
            
            IDbTransaction transaction = externalTransaction;
            
            // Если внешней транзакции нет (т.е. вызов был совершен из SyncProvider), то создаем ее
            if (transaction is null)
                transaction = connection.BeginTransaction();

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
            
            // фильтрация по столбцам, игнорируем значения которых нет в structure, но есть в values
            var values = dbData.List.Select(row =>
                row.Where(cell => columns.Contains(cell.Column))
                    .Select(x => x.Value));

            var valuesSql = SqlMappingExtensions.ToSqlValuesForInsert(values);

            string sql = @$"
                INSERT INTO dbo.{tableName} ({columnsSql})
                VALUES {valuesSql}";
            
            
            try
            {
                await connection.ExecuteAsync(sql, transaction: transaction);

                if (isInternalTransaction) 
                    transaction.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error inserting record to database");
                if (isInternalTransaction)
                    transaction.Rollback();

                throw;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inserting record to database");
            throw;
        }
        finally
        {
            if (externalConnection == null)
                connection.Dispose();
        }
    }

    public async Task RotateDirectoryDataAsync(string identifier, VersionInfoDto apiVersion, string currentVersion, StructureDto dbStructure, DataDto dbData, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        string actualTableName = $"{identifier}_Actual";
        string archiveTableName = $"{identifier}_History";
        
        var existingInDb = dbStructure.Columns.Select(c => c.Name).ToList();
        existingInDb.Add("SYS_RECORDID");
        existingInDb.Add("SYS_HASH");
        
        var columns = dbData.List.First()
            .Select(x => x.Column)
            .Where(c => existingInDb.Contains(c))
            .ToList();
        
        var columnsSql = SqlMappingExtensions.ToSqlColumnsForInsert(columns);

        string archiveTableSql = @$"
            INSERT INTO dbo.{archiveTableName} ({columnsSql}, [Version], [Archive_Date])
            SELECT {columnsSql}, {currentVersion}, GETDATE()
            FROM dbo.{actualTableName} WITH (UPDLOCK, HOLDLOCK, ROWLOCK)";

        string truncateActualTableSql = $@"
            TRUNCATE TABLE dbo.[{actualTableName}]";
        
        string versionTableSql = $@"
            UPDATE dbo.Directory_Actual_Version
            SET CurrentVersion = @NewVersion,
                LastUpdate = GETDATE()
            WHERE Code = @VersionCode";

        var paramForversionTableSql = new
        {
            VersionCode = identifier,
            NewVersion = apiVersion.Version
        };
        
        try
        {
            await connection.ExecuteAsync(archiveTableSql, transaction: transaction);
            
            await connection.ExecuteAsync(truncateActualTableSql, transaction: transaction);

            await InsertRecordsToDbAsync(identifier, dbData, dbStructure, cancellationToken, connection, transaction);
            
            await connection.ExecuteAsync(versionTableSql, paramForversionTableSql, transaction: transaction);
            
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
            string nullabillity = "NULL";
            return $"[{c.Name}] {sqlType} {nullabillity}";
        });
        
        string allColumnsSql = string.Join(",", columnDefinitions);
        
        string sql = $@"
        IF OBJECT_ID('{actualTable}') IS NULL
        CREATE TABLE {actualTable}
        (
            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            [SYS_RECORDID] NVARCHAR(MAX) NULL, 
            [SYS_HASH] NVARCHAR(MAX) NULL,
            {allColumnsSql}
        );

        IF OBJECT_ID('{historyTable}') IS NULL
        CREATE TABLE [{historyTable}]
        (
            [Id] BIGINT NOT NULL PRIMARY KEY,
            [SYS_RECORDID] NVARCHAR(255) NULL, 
            [SYS_HASH] NVARCHAR(255) NULL,
            {allColumnsSql},
            [Version] NVARCHAR(50) NOT NULL,
            [Archive_Date] DATETIME NOT NULL
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
