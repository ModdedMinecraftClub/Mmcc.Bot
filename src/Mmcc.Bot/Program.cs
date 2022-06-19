using System;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.Bot;
using Mmcc.Bot.Behaviours;
using Mmcc.Bot.Caching;
using Mmcc.Bot.Commands;
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
using Mmcc.Bot.Middleware;
using Mmcc.Bot.Mojang;
using Mmcc.Bot.Polychat;
using Mmcc.Bot.Polychat.Networking;
using Mmcc.Bot.RemoraAbstractions;
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
    .ConfigureServices((hostContext, services) =>
    {
        // config;
        services.ConfigureBot(hostContext);

        // db;
        services.AddBotDatabaseContext();

        services.AddSingleton<IColourPalette, TailwindColourPalette>();

        // FluentValidation;
        services.AddValidatorsFromAssemblyContaining<GetExpiredActions>();
        services.AddValidatorsFromAssemblyContaining<DiscordSettings>();
        services.AddValidatorsFromAssemblyContaining<MySqlSettingsValidator>();
        
        services.AddAppInsights(hostContext);

        // MediatR;
        services.AddMediatR(typeof(CreateFromDiscordMessage), typeof(PolychatRequest<>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // Mmcc.Bot.X projects;
        services.AddMmccCaching();
        services.AddMojangApi();
        services.AddPolychat(hostContext.Configuration.GetSection("Ssmp"));

        // Remora.Discord bot setup;
        services.AddRemoraAbstractions();
        services.AddBotMiddlewares();
        services.AddBotCommands();
        services.AddBotGatewayEventResponders();
        services.AddDiscordCaching();
        services.AddBotBackgroundServices();

        services.AddHangfire();
    })
    .AddDiscordService(provider =>
    {
        var discordConfig = provider.GetRequiredService<DiscordSettings>();
        return discordConfig.Token;
    })
    .UseSerilog(LoggerSetup.ConfigureLogger)
    .UseDefaultServiceProvider(options => options.ValidateScopes = true)
    .Build();

try
{
    Log.Information("Starting the host...");
    
    if (host.Services.GetRequiredService<IConfiguration>().GetValue<bool>(BotCommandLineArgs.Migrate))
    {
        Log.Information("Migrating the database...");

        await host.Migrate();
        
        Log.Information("Database migrated successfully");
    }
    
    await host.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}