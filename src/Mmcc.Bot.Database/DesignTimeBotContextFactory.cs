using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Mmcc.Bot.Database.Settings;

namespace Mmcc.Bot.Database
{
    /// <inheritdoc />
    public class DesignTimeBotContextFactory : IDesignTimeDbContextFactory<BotContext>
    {
        /// <inheritdoc />
        public BotContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Mmcc.Bot"))
                .AddJsonFile("appsettings.Development.json")
                .Build();

            // get connection string;
            var optionsBuilder = new DbContextOptionsBuilder<BotContext>();
            var boundConfig = config.GetSection("MySql").Get<MySqlSettings>();
            var connString =
                $"Server={boundConfig.ServerIp};Port={boundConfig.Port};Database={boundConfig.DatabaseName};Uid={boundConfig.Username};Pwd={boundConfig.Password};Allow User Variables=True";
            optionsBuilder.UseMySql(
                connString,
                ServerVersion.Parse("10.4.11-mariadb"),
                b => b.MigrationsAssembly("Mmcc.Bot.Database"));

            return new BotContext(optionsBuilder.Options);
        }
    }
}