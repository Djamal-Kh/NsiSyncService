namespace NsiSyncService.Core.DTOs;

public record VersionInfoDto()
{
    public string Version { get; set; }
    public string CreateDate { get; set; }
    public string PublishDate { get; set; }
    public string LastUpdate { get; set; }
    public PassportDto Passport { get; set; }
    public bool Archive { get; set; }

    // Метод для преобразования предполагаемой даты из string в DateTime
    public DateTime CreateDateTime =>
        DateTime.TryParse(CreateDate, out DateTime createDateTime)
            ? createDateTime
            : throw new Exception("Publish date cannot be parsed");
}

public record PassportDto()
{
    public string Type { get; set; }
    public string Version { get; set; }
    public string CreateDate { get; set; }
    public string PublishDate { get; set; }
    public string Name { get; set; }
    public string Annotation { get; set; }
    public string Law { get; set; }
    public int? GroupId { get; set; }
}