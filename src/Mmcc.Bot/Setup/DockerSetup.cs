using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mmcc.Bot.Database;
using Serilog;

namespace Mmcc.Bot.Setup;

public static class DockerSetup
{
    public static async Task SetupDocker(IHost host)
    {
        Log.Information("Detected Docker, migrating the database...");

        using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BotContext>().Database;

        await db.MigrateAsync();
        
        Log.Information("Database migrated successfully");
    }
}