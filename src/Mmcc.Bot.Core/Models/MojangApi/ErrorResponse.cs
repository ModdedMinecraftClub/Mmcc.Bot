namespace Mmcc.Bot.Core.Models.MojangApi
{
    /// <summary>
    /// Represents an error response.
    /// </summary>
    public interface IErrorResponse
    {
        /// <summary>
        /// Error name.
        /// </summary>
        public string Error { get; }
        
        /// <summary>
        /// Error message.
        /// </summary>
        public string ErrorMessage { get; }
    }
    
    /// <inheritdoc cref="IErrorResponse"/>
    public record ErrorResponse
    (
        string Error,
        string ErrorMessage
    ) : IErrorResponse;
}