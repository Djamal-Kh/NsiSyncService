using NsiSyncService.Core.DTOs;

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

    public static string ToSqlValuesForInsert(this IEnumerable<IEnumerable<string>> values)
    {
        var rows = values.Select(row =>
        {
            var processedRow = row.Select(record =>
            {
                if (string.IsNullOrEmpty(record))
                    return "NULL";

                if (DateTime.TryParse(record, out DateTime date))
                    return $"'{date:yyyy-MM-dd HH:mm:ss}'";

                return $"N'{record.Replace("'", "''")}'";
            });
            
            return $"({string.Join(", ", processedRow)})";
        });
        
        return string.Join(", ", rows);
    }

    public static string ToSqlColumnsForInsert(this List<string> columnNames)
    {
        return string.Join(", ", columnNames.Select(c => $"[{c}]"));
    }
}