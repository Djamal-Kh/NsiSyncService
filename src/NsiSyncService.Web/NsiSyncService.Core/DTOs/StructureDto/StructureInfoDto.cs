namespace NsiSyncService.Core.DTOs.StructureDto;

public record StructureInfoDto()
{
    public string Parent { get; set; }
    public string DisplayValue { get; set; }
    public string Sorting { get; set; }
    public bool IsHierarchical { get; set; }
    public List<Columns> Columns { get; set; }
    public List<Keys> Keys { get; set; }
}

public  record Columns
{
    public string Name { get; set; }
    public string Alias { get; set; }
    public string Description { get; set; }
    public string DataType { get; set; }
    public int Number { get; set; }
    public bool EmptyAllowed { get; set; }
    public bool Visible { get; set; }
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
    public int MaxIntPartLenght { get; set; }
    public int MaxFracPartLenght { get; set; }
}

public  record Keys
{
    public string Field { get; set; }
    public string Type { get; set; }
    public Reference Reference { get; set; }
    public object? ListStructure { get; set; }
}

public record Reference(int id, string version, string field);