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
        // Ищем запись из внешнего API
        var latestApiRecords = await _nsiApiClientService.GetLastVersionFromApiAsync(identifier, cancellationToken);
        
        _logger.LogInformation("Latest version record with identifier = {identifier} from API: {Version}", 
            identifier, latestApiRecords.Version);
        
        // Ищем запись в БД
        var currentDbVersions = await _nsiDirectoryRepository.GetLastVersionFromDbAsync(identifier, cancellationToken);

        // Если запись не встречается в БД, то тогда добавляем ее в БД
        if (currentDbVersions is null)
        {
            await _nsiDirectoryRepository.InsertRecordToDb(identifier, latestApiRecords, cancellationToken);
            return;
        }
        
        // Если записи из API и из БД совпадают, то ничего не меняем
        if (latestApiRecords.Version == currentDbVersions)
        {
            _logger.LogInformation("Version {version} in the record with identifier = {identifier} latest", 
                currentDbVersions, identifier);
            return;
        }
        
        // Если же все-таки записи не совпадают, значит в API находится более актуальная информация, 
        // поэтому надо обновить записи в таблицах
        await _nsiDirectoryRepository.RotateDirectoryDataAsync(identifier, latestApiRecords, cancellationToken);
        
        _logger.LogInformation("Update Record in Database with identifier = {identifier}. " +
                               "Directory latest version: {newVersion}, arhived version: {oldVersion}", 
            identifier, latestApiRecords.Version, currentDbVersions);
        
        return;
    }
}

