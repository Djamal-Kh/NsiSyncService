using System.Text.Json.Serialization;

public record StructureDto
{
    public string? Parent { get; set; }
    public string? DisplayValue { get; set; }
    public string? Sorting { get; set; }
    public bool IsHierarchical { get; set; }
    public List<ColumnsDto> Columns { get; set; } = new();
    public List<KeysDto> Keys { get; set; } = new();
}

public record ColumnsDto
{
    public string Name { get; set; } = "";
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public string DataType { get; set; } = "";
    public string? ContentType { get; set; }
    public int? Number { get; set; }
    public bool EmptyAllowed { get; set; }
    public bool Visible { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
}

public record KeysDto
{
    public string Field { get; set; } = "";
    public string Type { get; set; } = "";
    public ReferenceDto? Reference { get; set; }
}

public record ReferenceDto
{
    public int Id { get; set; }
    public int Version { get; set; }
    public string Field { get; set; } = "";
}