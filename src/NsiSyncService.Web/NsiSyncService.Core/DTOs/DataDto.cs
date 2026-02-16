namespace NsiSyncService.Core.DTOs;

public record DataDto()
{
    public string Result { get; set; }
    public string ResultText { get; set; }
    public string ResultCode { get; set; }
    public int Total { get; set; }
    public List<List<ColumnValue>> List { get; set; }
}

public record ColumnValue()
{
    public string Column { get; set; }
    public string Value { get; set; }
}