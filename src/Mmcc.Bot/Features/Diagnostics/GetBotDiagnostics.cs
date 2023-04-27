using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Results;

namespace Mmcc.Bot.Features.Diagnostics;

/// <summary>
/// Pings all network resources to check specified in <see cref="IDiagnosticsSettings"/>.
/// </summary>
public sealed class GetBotDiagnostics
{
    public record struct Query : IRequest<Result<IList<QueryResult>>>;

    public sealed record QueryResult(string Name, string Address, IPStatus Status, long? RoundtripTime);
    
    public sealed class Handler : IRequestHandler<Query, Result<IList<QueryResult>>>
    {
        private readonly IDiagnosticsSettings _settings;

        public Handler(IDiagnosticsSettings settings) 
            => _settings = settings;

        public async Task<Result<IList<QueryResult>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<QueryResult>(_settings.NetworkResourcesToCheck.Count);
                foreach (var (name, address) in _settings.NetworkResourcesToCheck)
                {
                    var pingResult = await PingResource(name, address);
                    
                    results.Add(pingResult);
                }

                return Result<IList<QueryResult>>.FromSuccess(results);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private async Task<QueryResult> PingResource(string name, string address)
        {
            var ping = new Ping();
            var options = new PingOptions
            {
                DontFragment = true
            };
            var buffer = new byte[32];
            var reply = await ping.SendPingAsync(address, _settings.Timeout, buffer, options);

            return new(Name: name, Address: reply.Address.ToString(), Status: reply.Status, RoundtripTime: reply.RoundtripTime);
        }
    }
}