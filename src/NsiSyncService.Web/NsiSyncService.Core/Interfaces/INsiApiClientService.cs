using NsiSyncService.Core.DTOs;

namespace NsiSyncService.Core.Interfaces;

public interface INsiApiClientService
{
    public Task<VersionInfoDto> GetLastVersionFromApiAsync(string identifier, CancellationToken cancellationToken);
    public Task<VersionsRequestDto> GetVersionsFromApiAsync(string identifier, CancellationToken cancellationToken,
        int page = 1, int size = 200);
}