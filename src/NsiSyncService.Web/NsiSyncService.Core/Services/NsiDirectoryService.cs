using NsiSyncService.Core.DTOs.VersionsDto;
using NsiSyncService.Core.Interfaces;

namespace NsiSyncService.Core.Services;

public class NsiDirectoryService : INsiDirectoryService
{
    public Task<VersionInfoDto> GetLastVersionFromDbAsync(string identifier, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}