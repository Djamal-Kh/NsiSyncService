namespace NsiSyncService.Core.DTOs;

public record VersionsRequestDto()
{
    public string Result { get; set; }
    public string ResultText { get; set; }
    public string ResultCode { get; set; }
    public int Total { get; set; }
    public List<VersionInfoDto> List { get; set; }
}