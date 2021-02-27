using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediatR;

namespace Mmcc.Bot.Infrastructure.Queries.Diagnostics
{
    /// <summary>
    /// Query to get drives.
    /// </summary>
    public class GetDrives
    {
        /// <summary>
        /// Query to get drives.
        /// </summary>
        public class Query : IRequest<IList<QueryResult>>
        {
        }
        
        /// <summary>
        /// Result of the drive query.
        /// </summary>
        public class QueryResult
        {
            public string Name { get; set; } = null!;
            public DriveType DriveType { get; set; }
            public string Label { get; set; } = null!;
            public string DriveFormat { get; set; } = null!;
            public float GigabytesFree { get; set; }
            public double PercentageUsed { get; set; }
            public float GigabytesTotalSize { get; set; }
        }
        
        /// <inheritdoc />
        public class Handler : RequestHandler<Query, IList<QueryResult>>
        {
            /// <inheritdoc />
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
                        PercentageUsed = (double) (d.TotalSize - d.AvailableFreeSpace) / d.TotalSize * 100,
                        GigabytesTotalSize = d.TotalSize / 1024f / 1024f / 1024f
                    })
                    .ToList();
            }
        }
    }
}