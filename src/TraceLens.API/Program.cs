using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using TraceLens.API.Hubs;
using TraceLens.API.Jobs;
using TraceLens.API.Middleware;
using TraceLens.API.Notifications;
using TraceLens.Application;
using TraceLens.Application.Common.Interfaces;
using TraceLens.Infrastructure;
using TraceLens.Infrastructure.Auth;
using TraceLens.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services.AddApplication();

// Redis distributed cache
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "TraceLens:";
});
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = jwtSection["Issuer"] ?? "TraceLens.API";
var jwtAudience = jwtSection["Audience"] ?? "TraceLens.Client";
builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddSingleton<IAuthService, JwtAuthService>();

builder.Services.AddOpenIddict()
    .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
    .AddServer(o =>
    {
        o.SetTokenEndpointUris("/connect/token");
        o.AllowPasswordFlow().AllowRefreshTokenFlow();
        o.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
        o.UseAspNetCore().EnableTokenEndpointPassthrough().DisableTransportSecurityRequirement();
    })
    .AddValidation(o => { o.UseLocalServer(); o.UseAspNetCore(); });

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer, ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionString not configured");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();
builder.Services.AddScoped<CheckStaleBatchesJob>();

builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
builder.Services.AddSignalR();
builder.Services.AddScoped<IBatchNotifier, SignalRBatchNotifier>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(o => o.AddPolicy("Frontend", p => p
    .WithOrigins("http://localhost:4200", "http://localhost:3000")
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMcp("/mcp");
app.MapHub<BatchHub>("/hubs/batches");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);

        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync("api") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api", DisplayName = "TraceLens API", Resources = { "tracelens-api" }
            });
            Log.Information("OpenIddict scope 'api' seeded");
        }

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        if (await manager.FindByClientIdAsync("tracelens-spa") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "tracelens-spa", ClientSecret = "tracelens-secret",
                DisplayName = "TraceLens Angular SPA",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                }
            });
            Log.Information("OpenIddict client 'tracelens-spa' seeded");
        }

        var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        jobManager.AddOrUpdate<CheckStaleBatchesJob>(
            "check-stale-batches",
            job => job.Run(CancellationToken.None),
            Cron.Hourly);
        Log.Information("Recurring job 'check-stale-batches' scheduled (hourly)");

        Log.Information("Database migrated and seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database migration skipped: {Message}", ex.Message);
    }
}

app.Run();
