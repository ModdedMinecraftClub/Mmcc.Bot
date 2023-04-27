using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediatR;

namespace Mmcc.Bot.Features.Diagnostics;

/// <summary>
/// Gets the drives diagnostics.
/// </summary>
public sealed class GetDrivesDiagnostics
{
    /// <summary>
    /// Query to get drives.
    /// </summary>
    public sealed class Query : IRequest<IList<QueryResult>>
    {
    }

    /// <summary>
    /// Drive diagnostics.
    /// </summary>
    public sealed class QueryResult
    {
        public string Name { get; set; } = null!;
        public DriveType DriveType { get; set; }
        public string Label { get; set; } = null!;
        public string DriveFormat { get; set; } = null!;
        public float GigabytesFree { get; set; }
        public double PercentageUsed { get; set; }
        public float GigabytesTotalSize { get; set; }
    }

    public sealed class Handler : RequestHandler<Query, IList<QueryResult>>
    {
        protected override IList<QueryResult> Handle(Query request)
        {
            var allDrives = DriveInfo.GetDrives();
            return allDrives
                .Where(d => d.IsReady)
                .Select(d => new QueryResult
                {
                    Name = d.Name,
                    DriveType = d.DriveType,
                    Label = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "None" : d.VolumeLabel,
                    DriveFormat = d.DriveFormat,
                    GigabytesFree = d.AvailableFreeSpace / 1024f / 1024f / 1024f,
                    PercentageUsed = (double)(d.TotalSize - d.AvailableFreeSpace) / d.TotalSize * 100,
                    GigabytesTotalSize = d.TotalSize / 1024f / 1024f / 1024f
                })
                .ToList();
        }
    }
}