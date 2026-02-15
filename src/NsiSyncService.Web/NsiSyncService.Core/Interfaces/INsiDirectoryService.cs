using NsiSyncService.Core.DTOs.VersionsDto;

namespace NsiSyncService.Core.Interfaces;

public interface INsiDirectoryService
{
    Task<VersionInfoDto> GetLastVersionFromDbAsync(string identifier, CancellationToken cancellationToken = default);
}