using NsiSyncService.Core;
using NsiSyncService.Core.Interfaces;
using NsiSyncService.Core.Services;
using NsiSyncService.Infrastructure;
using NsiSyncService.Infrastructure.BackgroundServices;
using NsiSyncService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<INsiApiClientService, NsiApiClientService>(client =>
    {
        client.BaseAddress = new Uri("https://nsi.ffoms.ru/nsi-int/");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Этот код отключает проверку сертификатов
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });

builder.Services.AddTransient<DbInitializer>();
builder.Services.AddHostedService<NsiSyncWorker>();
builder.Services.AddScoped<ISyncProvider, SyncProvider>();
builder.Services.AddScoped<INsiDirectoryRepository, NsiDirectoryRepository>();
builder.Services.AddScoped<IDbConnectionFactory, SqlServerConnectionFactory>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();