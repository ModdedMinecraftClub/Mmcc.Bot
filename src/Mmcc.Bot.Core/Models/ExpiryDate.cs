using System;

namespace Mmcc.Bot.Core.Models
{
    /// <summary>
    /// Represents an expiry date.
    /// </summary>
    public class ExpiryDate
    {
        /// <summary>
        /// Whether the action is permanent.
        /// </summary>
        public bool IsPermanent => Value is null;
        
        /// <summary>
        /// Expiry date in UNIX time format. Set to <code>null</code> if permanent.
        /// </summary>
        public long? Value { get; init; }
    }
}