namespace NsiSyncService.Core.DTOs.VersionsDto;

public record VersionInfoDto()
{
    public string Version { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public Passport Passport { get; set; }
    public bool Archive { get; set; }
}

public record Passport()
{
    public string Type { get; set; }
    public string Version { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime PublishDate { get; set; }
    public string Name { get; set; }
    public string Annotation { get; set; }
    public string Law { get; set; }
    public int GroupId { get; set; }
}