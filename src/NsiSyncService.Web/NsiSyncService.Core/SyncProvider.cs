using Microsoft.Extensions.Logging;
using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Core;

public class SyncProvider : ISyncProvider
{
    private readonly INsiApiClientService _nsiApiClientService;
    private readonly INsiDirectoryRepository _nsiDirectoryRepository;
    private readonly ILogger<SyncProvider> _logger;

    public SyncProvider(
        INsiApiClientService nsiApiClientService, 
        ILogger<SyncProvider> logger, 
        INsiDirectoryRepository nsiDirectoryRepository)
    {
        _nsiApiClientService = nsiApiClientService;
        _logger = logger;
        _nsiDirectoryRepository = nsiDirectoryRepository;
    }

    public async Task SyncReferenceAsync(string identifier, CancellationToken cancellationToken = default)
    {
        // Ищем последнюю версию из API
        var latestApiVersion = await _nsiApiClientService.GetLastVersionFromApiAsync(identifier, cancellationToken);
        
        // Ищем версию в БД
        var currentDbVersion = await _nsiDirectoryRepository.GetLastVersionFromDbAsync(identifier, cancellationToken);

        // Если в БД есть соответствующие таблицы и версии с API и с БД совпадают, то переходим к следующему идентификатору
        if (currentDbVersion is not null && latestApiVersion.Version == currentDbVersion)
        {
            _logger.LogInformation("Version {version} in the record with identifier = {identifier} latest", 
                currentDbVersion, identifier);
            return;
        }
        
        // вытаскиваем данные и структуру таблицы из API для заполнения таблицы 
        var apiData = await _nsiApiClientService.GetDataFromApiAsync(identifier, cancellationToken);
        var apiStructure = await _nsiApiClientService.GetStructureFromApiAsync(identifier, cancellationToken);

        // если в БД не существует таблица с соответствующим идентификатором, то создаем ее и наполняем БД данными
        if (currentDbVersion is null)
        {
            await _nsiDirectoryRepository.CreateTablesAsync(identifier, apiStructure, cancellationToken);
            await _nsiDirectoryRepository.InsertRecordsToDbAsync(identifier, apiData, apiStructure, cancellationToken);
            await _nsiDirectoryRepository.AddVersionAsync(identifier, latestApiVersion, cancellationToken);
            return;
        }
            
        // если в БД есть таблица и данные, но они неактульны, то переносим данные из актульальной таблицы в архивную,
        // а актуальную сначала очищаем и потом заполняем уже актуальными данными взятыми из API
        await _nsiDirectoryRepository.RotateDirectoryDataAsync(
            identifier, 
            latestApiVersion, 
            currentDbVersion, 
            apiStructure, 
            apiData, 
            cancellationToken);
        
        _logger.LogInformation("Update Record in Database with identifier = {identifier}. " +
                               "Directory latest version: {newVersion}, arhived version: {oldVersion}", 
            identifier, latestApiVersion.Version, currentDbVersion);
        
        return;
    }
}

