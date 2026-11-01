using MassTransit;
using TraceLens.Infrastructure;
using TraceLens.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// Registers ApplicationDbContext (via AddDbContext) + IApplicationDbContext mapping.
// Same DI used by TraceLens.API — single source of truth for persistence.
builder.Services.AddInfrastructure(builder.Configuration);

// RabbitMQ config (same shape as API's appsettings.RabbitMq)
var rabbitSection = builder.Configuration.GetSection("RabbitMq");
var rabbitHost = rabbitSection["Host"] ?? "localhost";
var rabbitVHost = rabbitSection["VirtualHost"] ?? "/";
var rabbitUser = rabbitSection["Username"] ?? "guest";
var rabbitPass = rabbitSection["Password"] ?? "guest";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BatchCreatedConsumer>();
    x.AddConsumer<BatchStatusChangedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, rabbitVHost, h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        // Auto-creates a queue per consumer and binds it to the corresponding exchange.
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();
