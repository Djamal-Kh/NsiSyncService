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
        // вызвать _nsiApiClientService и получить актуальную версию из API
        var latestApiRecords = await _nsiApiClientService.GetLastVersionFromApiAsync(identifier, cancellationToken);
        
        _logger.LogInformation("Latest version from API: {Version}", latestApiRecords.Version);
        
        // вызвать _nsiDirectoryService и получить актуальную версию из БД
        var currentDbVersions = await _nsiDirectoryRepository.GetLastVersionFromDbAsync(identifier, cancellationToken);

        if (currentDbVersions is null)
        {
            await _nsiDirectoryRepository.InsertRecordToDb(identifier, latestApiRecords, cancellationToken);
            return;
        }
        
        // если же в бд нашлась подобная запись, то сравниваем их
        if (latestApiRecords.Version == currentDbVersions)
        {
            _logger.LogInformation("Version in the Db is latest");
            return;
        }
        
        await _nsiDirectoryRepository.RotateDirectoryDataAsync(identifier, latestApiRecords, cancellationToken);
        _logger.LogInformation("Update Record in Database. Directory latest version: {newVersion}, arhived version: {oldVersion}", 
            latestApiRecords.Version, currentDbVersions);
    }
}

