using NsiSyncService.Infrastructure;
using NsiSyncService.Infrastructure.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddTransient<DbInitializer>();
builder.Services.AddHostedService<NsiSyncWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();