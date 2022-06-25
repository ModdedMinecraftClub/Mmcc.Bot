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
    private readonly PeriodicTimer _timer;

    /// <summary>
    /// Instantiates a new instance of <see cref="TimedBackgroundService{TLogger}"/>.
    /// </summary>
    /// <param name="periodBetweenIterations"><see cref="TimeSpan"/> between each iteration of <see cref="OnExecute"/> is run.</param>
    /// <param name="logger">The logger.</param>
    protected TimedBackgroundService(TimeSpan periodBetweenIterations, ILogger<TLogger> logger)
    {
        _logger = logger;
        _timer = new(periodBetweenIterations);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Started service: {Service}", typeof(TLogger).ToString());
        
        while (await _timer.WaitForNextTickAsync(ct) && !ct.IsCancellationRequested)
        {
            try
            {
                await OnExecute(ct);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "An exception has occurred while running {Service}", typeof(TLogger).ToString());
            }
        }
    }

    /// <summary>
    /// Method to be executed on every iteration of the timed background service.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected abstract Task OnExecute(CancellationToken ct);
}