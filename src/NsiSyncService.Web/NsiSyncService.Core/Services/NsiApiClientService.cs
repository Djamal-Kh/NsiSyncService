using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using NsiSyncService.Core.DTOs;
using NsiSyncService.Core.Extensions.ExceptionExtensions;
using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Core.Services;

public class NsiApiClientService : INsiApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NsiApiClientService> _logger;
    
    public NsiApiClientService(HttpClient httpClient, ILogger<NsiApiClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<VersionInfoDto> GetLastVersionFromApiAsync(string identifier, CancellationToken cancellationToken)
    {
        var allVersions = new List<VersionInfoDto>();
        var page = 1;
        var pageSize = 200;
        var totalPages = 1;

        do
        {
            var response = await GetVersionsFromApiAsync(identifier, cancellationToken, page, pageSize);
            
            if (response.List != null && response.List.Any())
            {
                allVersions.AddRange(response.List);
                
                if (response.Total > 0)
                {
                    totalPages = (int)Math.Ceiling(response.Total / (double)pageSize);
                }
            }
            else
            {
                break;
            }

            page++;
            
            if (page <= totalPages)
            {
                await Task.Delay(500);
            }

        } while (page <= totalPages);

        if(!allVersions.Any())
            throw new ResourceNotFoundException();
        
        return allVersions
            .OrderByDescending(v => v.CreateDateTime)
            .FirstOrDefault();
    }
    
    public async Task<VersionsRequestDto> GetVersionsFromApiAsync(
        string identifier,
        CancellationToken cancellationToken, 
        int page = 1, 
        int size = 200)
    {
        var url = $"https://nsi.ffoms.ru/nsi-int/api/versions?identifier={identifier}&page={page}&size={size}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        // Если статус код не "Ok" (200)
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API вернуло ошибку {StatusCode}. Тело ответа: {Content}", response.StatusCode, errorContent);
            return new VersionsRequestDto { List = new List<VersionInfoDto>() };
        }
        
        var result = await response.Content.ReadFromJsonAsync<VersionsRequestDto>(cancellationToken);
        
        return result ?? new VersionsRequestDto { List = new List<VersionInfoDto>() };
    }
}