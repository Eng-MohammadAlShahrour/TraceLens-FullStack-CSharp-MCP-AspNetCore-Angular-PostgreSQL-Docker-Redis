using Microsoft.EntityFrameworkCore;
using Serilog;
using TraceLens.API.Middleware;
using TraceLens.Application;
using TraceLens.Infrastructure;
using TraceLens.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(o => o.AddPolicy("Frontend", p => p
    .WithOrigins("http://localhost:4200", "http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "TraceLens API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
        Log.Information("Database migrated and seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database migration skipped: {Message}", ex.Message);
    }
}

app.Run();