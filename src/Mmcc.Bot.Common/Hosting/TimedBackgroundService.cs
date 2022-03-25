using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mmcc.Bot.Common.Hosting;

/// <summary>
/// Represents a timed background service
/// </summary>
public abstract class TimedBackgroundService<TLogger> : BackgroundService
{
    private readonly ILogger<TLogger> _logger;
    private readonly int _timeBetweenIterationsInMillis;

    /// <summary>
    /// Instantiates a new instance of <see cref="TimedBackgroundService{TLogger}"/>.
    /// </summary>
    /// <param name="timeBetweenIterationsInMillis">Time between each iteration of <see cref="OnExecute"/> is ran.</param>
    /// <param name="logger">The logger.</param>
    protected TimedBackgroundService(int timeBetweenIterationsInMillis, ILogger<TLogger> logger)
    {
        _timeBetweenIterationsInMillis = timeBetweenIterationsInMillis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Started service: {Service}", typeof(TLogger).ToString());
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await OnExecute(ct);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "An exception has occurred while running {Service}", typeof(TLogger).ToString());
            }
            
            await Task.Delay(_timeBetweenIterationsInMillis, ct);
        }
    }

    /// <summary>
    /// Method to be executed on every iteration of the timed background service.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected abstract Task OnExecute(CancellationToken ct);
}