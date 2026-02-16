namespace NsiSyncService.Core.Extensions;

public static class SqlMappingExtensions
{
    public static string ToSqlType(this ColumnsDto columnsDto)
    {
        return columnsDto.DataType switch
        {
            "VARCHAR" => $"NVARCHAR({(columnsDto.MaxLength > 0 ? columnsDto.MaxLength.ToString() : "MAX")})",
            "INTEGER" => "INT",
            "DATE" => "DATE",
            _ => "NVARCHAR(MAX)"
        };
    }
}