namespace NsiSyncService.Core.DTOs;

public record DataDto()
{
    public string Column { get; set; }
    public string Value { get; set; }
}