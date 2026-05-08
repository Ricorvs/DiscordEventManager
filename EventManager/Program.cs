using EventManager.Calendar;
using EventManager.DailyTasks;
using EventManager.DailyTasks.RefreshThreads;
using EventManager.DbContext;
using EventManager.EventRepeat;
using EventManager.Events;
using EventManager.Events.Services;
using EventManager.GenerateDates;
using EventManager.GuildConfiguration;
using EventManager.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = WebApplication.CreateBuilder(args);
var connectionstring = builder.Configuration.GetConnectionString("EventManagerDataContext")
                    ?? throw new InvalidOperationException("Connection string 'EventManagerDataContext' not found.");
builder.Services.AddDbContextFactory<EventManagerDbContext>(options =>
    options.UseMySql(connectionstring, ServerVersion.AutoDetect(connectionstring)));
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services
    .AddDiscordGateway(options =>
          options.Intents = GatewayIntents.Guilds
                          | GatewayIntents.GuildMessages
                          | GatewayIntents.DirectMessages
                          | GatewayIntents.MessageContent
                          | GatewayIntents.GuildScheduledEvents
                          | GatewayIntents.DirectMessageReactions
                          | GatewayIntents.GuildMessageReactions)
    .AddGatewayHandlers(typeof(Program).Assembly)
    .AddApplicationCommands()
    .AddTransient<DiscordEventGatewayHandler>()
    .AddTransient<EventService>()
    .AddTransient<EventRepeatConfigurationService>()
    .AddSingleton<EventRegistrationService>()
    .AddTransient<EventRepeatService>()
    .AddTransient<CalendarService>()
    .AddTransient<RefreshThreadsService>()
    .AddTransient<GenerateDatesService>()
    .AddTransient<GuildConfigurationService>()
    .AddSingleton<DailyTasksBackgroundService>()
    .AddSingleton<IDailyGuildTask, RefreshThreadsDailyTask>()
    .AddSingleton<IDailyGuildTask, EventRepeatDailyTask>()
    .AddHostedService<StartupService>();

builder.Services.AddControllers();

var host = builder.Build();

await host.Services.GetRequiredService<DailyTasksBackgroundService>().StartAsync(CancellationToken.None);
host.AddModules(typeof(Program).Assembly);
host.MapControllers();

await host.RunAsync();