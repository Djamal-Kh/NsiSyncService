namespace NsiSyncService.Core.DTOs.DataDto;

public record DataDto()
{
    public string Result { get; set; }
    public string ResultText { get; set; }
    public string ResultCode { get; set; }
    public int Total { get; set; }
    public List<List<Record>> List { get; set; }
}

public record Record
{
    public string Column { get; set; }
    public string Value { get; set; }
}