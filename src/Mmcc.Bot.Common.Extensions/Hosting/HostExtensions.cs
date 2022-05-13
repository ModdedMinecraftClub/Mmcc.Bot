using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mmcc.Bot.Database;

namespace Mmcc.Bot.Common.Extensions.Hosting;

public static class HostExtensions
{
    public static async Task Migrate(this IHost host)
    {
        using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BotContext>().Database;

        await db.MigrateAsync();
    }
}