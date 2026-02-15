using NsiSyncService.Core.DTOs.VersionsDto;

namespace NsiSyncService.Core.Interfaces;

public interface INsiApiClientService
{
    public Task<VersionsResponseDto> GetVersionsFromApiAsync(string identifier, CancellationToken cancellationToken,
        int page = 1, int size = 200);
    
    public Task<VersionInfoDto> GetLastVersionFromApiAsync(string identifier, CancellationToken cancellationToken);
}