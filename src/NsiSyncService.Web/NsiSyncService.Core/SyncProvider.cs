using Microsoft.Extensions.Logging;
using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Core;

public class SyncProvider : ISyncProvider
{
    private readonly INsiApiClientService _nsiApiClientService;
    private readonly INsiDirectoryRepository _nsiDirectoryRepository;
    private readonly ILogger<SyncProvider> _logger;

    public SyncProvider(INsiApiClientService nsiApiClientService, ILogger<SyncProvider> logger, INsiDirectoryRepository nsiDirectoryRepository)
    {
        _nsiApiClientService = nsiApiClientService;
        _logger = logger;
        _nsiDirectoryRepository = nsiDirectoryRepository;
    }

    public async Task SyncReferenceAsync(string identifier, CancellationToken cancellationToken = default)
    {
        // Ищем последнюю версию из API - done
        var latestApiVersion = await _nsiApiClientService.GetLastVersionFromApiAsync(identifier, cancellationToken);
        
        // Ищем версию в БД - done
        var currentDbVersion = await _nsiDirectoryRepository.GetLastVersionFromDbAsync(identifier, cancellationToken);

        // Если в БД есть соответствующие таблицы и версии с API и с БД совпадают, то переходим к следующему идентификатору
        if (currentDbVersion is not null && latestApiVersion.Version == currentDbVersion)
        {
            _logger.LogInformation("Version {version} in the record with identifier = {identifier} latest", 
                currentDbVersion, identifier);
            return;
        }
        
        // вытаскиваем данные из API для заполнения таблицы - done
        var dbDataForCreatingTable = await _nsiApiClientService.GetDataFromApiAsync(identifier, cancellationToken);

        // если раннее не создавали БД
        if (currentDbVersion is null)
        {
            var dbStructureForCreatingTable = await _nsiApiClientService.GetStructureFromApiAsync(identifier, cancellationToken);
            await _nsiDirectoryRepository.CreateTablesAsync(identifier, dbStructureForCreatingTable, cancellationToken);
            //await _nsiDirectoryRepository.AddVersionAsync(identifier, latestApiVersion, cancellationToken);
            await _nsiDirectoryRepository.InsertRecordToDbAsync(identifier, dbDataForCreatingTable, dbStructureForCreatingTable, cancellationToken);
            return;
        }
            
        // если надо обновить данные в таблицах
        await _nsiDirectoryRepository.RotateDirectoryDataAsync(identifier, latestApiVersion, dbDataForCreatingTable ,cancellationToken);
        
        _logger.LogInformation("Update Record in Database with identifier = {identifier}. " +
                               "Directory latest version: {newVersion}, arhived version: {oldVersion}", 
            identifier, latestApiVersion.Version, currentDbVersion);
        
        return;
    }
}

