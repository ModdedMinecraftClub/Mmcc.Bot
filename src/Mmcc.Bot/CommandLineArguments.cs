using Microsoft.Extensions.Configuration;

namespace Mmcc.Bot;

public class CommandLineArguments
{
    private readonly IConfiguration _configuration;

    public CommandLineArguments(IConfiguration configuration)
        => _configuration = configuration;

    public bool ShouldMigrate => _configuration.GetValue<bool>("migrate");
}