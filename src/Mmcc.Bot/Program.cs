using System;
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.Bot;
using Mmcc.Bot.Behaviours;
using Mmcc.Bot.Commands;
using Mmcc.Bot.Common;
using Mmcc.Bot.Common.Extensions.Hosting;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.CoreSetup;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Settings;
using Mmcc.Bot.EventResponders;
using Mmcc.Bot.EventResponders.Moderation.MemberApplications;
using Mmcc.Bot.Hosting;
using Mmcc.Bot.Hosting.Moderation;
using Mmcc.Bot.InMemoryStore.Stores;
using Mmcc.Bot.Interactions;
using Mmcc.Bot.Middleware;
using Mmcc.Bot.Notifications;
using Mmcc.Bot.Polychat;
using Mmcc.Bot.Polychat.Networking;
using Mmcc.Bot.Providers;
using Mmcc.Bot.RemoraAbstractions;
using Porbeagle;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Hosting.Extensions;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((context, builder) =>
    {
        if (context.HostingEnvironment.IsDevelopment())
        {
            builder.AddDebug();
        }
    })
    .AddDiscordService(provider => provider.GetRequiredService<DiscordSettings>().Token)
    .ConfigureServices((hostContext, services) =>
    {
        // config;
        services.ConfigureBot(hostContext);

        // db;
        services.AddInMemoryStores();
        services.AddBotDatabaseContext();

        services.AddSingleton<IColourPalette>();

        // FluentValidation;
        services.AddValidatorsFromAssemblyContaining<GetExpiredActions>();
        services.AddValidatorsFromAssemblyContaining<DiscordSettings>();
        services.AddValidatorsFromAssemblyContaining<MySqlSettingsValidator>();
        
        services.AddAppInsights(hostContext);

        // MediatR;
        services.AddMediatR(new [] { typeof(CreateFromDiscordMessage), typeof(PolychatRequest<>) }, cfg =>
        {
            cfg.WithEvaluator(t => t.GetCustomAttribute<ExcludeFromMediatrAssemblyScanAttribute>() is null);
        });
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(INotificationHandler<>), typeof(DiscordNotificationHandler<>));

        // Mmcc.Bot.X projects;
        services.AddPolychat(hostContext.Configuration.GetSection("Ssmp"));

        services.AddProviders();
        
        // Remora.Discord bot setup;
        services.AddRemoraAbstractions();
        services.AddBotMiddlewares();
        services.AddBotCommands();
        services.AddInteractions();
        services.AddBotGatewayEventResponders();
        services.AddDiscordCaching();
        services.AddBotBackgroundServices();
        services.AddScoped<IContextAwareViewManager, MessageStyleViewManager>();

        services.AddHangfire();
    })
    .UseSerilog(LoggerSetup.ConfigureLogger)
    .UseDefaultServiceProvider(options => options.ValidateScopes = true)
    .Build();

try
{
    Log.Information("Starting the host...");

    var shouldMigrate = host.Services.GetService<CommandLineArguments>()?.ShouldMigrate ?? false;
    
    if (shouldMigrate)
    {
        Log.Information("Migrating the database...");

        await host.Migrate();
        
        Log.Information("Database migrated successfully");
    }
    
    await host.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}