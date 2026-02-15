namespace NsiSyncService.Core.DTOs;

public sealed record ErrorDetailsDto()
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
}