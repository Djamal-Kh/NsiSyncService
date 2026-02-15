using Microsoft.Extensions.Logging;
using NsiSyncService.Core.DTOs.VersionsDto;
using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Core;

public class SyncProvider : ISyncProvider
{
    private readonly INsiApiClientService _nsiApiClientService;
    private readonly INsiDirectoryService _nsiDirectoryService;
    private readonly ILogger<SyncProvider> _logger;

    public SyncProvider(INsiApiClientService nsiApiClientService, ILogger<SyncProvider> logger, INsiDirectoryService nsiDirectoryService)
    {
        _nsiApiClientService = nsiApiClientService;
        _logger = logger;
        _nsiDirectoryService = nsiDirectoryService;
    }

    public async Task SyncReferenceAsync(string identifier, CancellationToken cancellationToken = default)
    {
        // вызвать _nsiApiClientService и получить актуальную версию из API
        var latestApiVersions = await _nsiApiClientService.GetLastVersionFromApiAsync(identifier, cancellationToken);
        
        _logger.LogInformation("Latest version from API: {Version}", latestApiVersions.Version);
        
        // вызвать _nsiDirectoryService и получить актуальную версию из БД
        var currentDbVersions = await _nsiDirectoryService.GetLastVersionFromDbAsync(identifier, cancellationToken);
        
        // сравнить актуальные версии из API и из БД
        if (latestApiVersions.Version == currentDbVersions.Version && latestApiVersions is not null)
        {
            _logger.LogInformation("Version in the Db is latest");
            return;
        }
        
        // начать загрузку из API и начать загрузку в БД 
        
        return;
    }
}

