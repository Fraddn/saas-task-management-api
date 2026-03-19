using Microsoft.EntityFrameworkCore;
using ProjectSaas.Worker.Configuration;
using ProjectSaas.Worker.EventHandlers;
using ProjectSaas.Worker.Persistence;
using ProjectSaas.Worker.Services;
using ProjectSaas.Worker.Services.Interfaces;
using ProjectSaas.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<OutboxProcessingOptions>(
    builder.Configuration.GetSection(OutboxProcessingOptions.SectionName));

builder.Services.AddDbContext<WorkerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOutboxEventDispatcher, OutboxEventDispatcher>();
builder.Services.AddScoped<IOutboxEventHandler, TicketCreatedEventHandler>();
builder.Services.AddScoped<IOutboxEventHandler, TicketHighPriorityEventHandler>();

builder.Services.AddHostedService<OutboxProcessorWorker>();

var host = builder.Build();
host.Run();