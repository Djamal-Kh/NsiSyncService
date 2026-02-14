using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace NsiSyncService.Infrastructure;

public class DbInitializer
{
    private readonly string _connectionString;
    
    public DbInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var scriptPath = Path.Combine(@"F:\IT files and 3D\C# Projects\NsiSyncService\src\NsiSyncService.Web\NsiSyncService.Infrastructure\DbScripts\init.sql");
        
        if(!File.Exists(scriptPath))
            throw new FileNotFoundException("init.sql", scriptPath);
        
        var script = File.ReadAllText(scriptPath);
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        await connection.ExecuteAsync(script, cancellationToken);
        
        await connection.CloseAsync();
    }
}