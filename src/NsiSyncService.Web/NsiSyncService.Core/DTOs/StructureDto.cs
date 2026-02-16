namespace NsiSyncService.Core.DTOs;

public record StructureDto()
{
    public string Name { get; set; }
    public string Alias { get; set; }
    public string Description { get; set; }
    public string DataType { get; set; }
    public string ContentType { get; set; }
    public string Number { get; set; }
    public string EmptyAllowed { get; set; }
    public string MinLength { get; set; }
    public string MaxLength { get; set; }
    public string MaxIntPartLength { get; set; }
    public string MaxFracPartLength { get; set; }
}