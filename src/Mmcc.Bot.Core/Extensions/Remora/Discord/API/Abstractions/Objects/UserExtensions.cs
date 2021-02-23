using System.Text;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Core.Extensions.Remora.Discord.API.Abstractions.Objects
{
    public static class UserExtensions
    {
        /// <summary>
        /// Gets a EmbedField representation of a Discord user.
        /// </summary>
        /// <param name="user">Discord user.</param>
        /// <returns>EmbedField representation of a Discord user.</returns>
        public static EmbedField GetEmbedField(this IUser user)
        {
            var value = new StringBuilder();
            value.AppendLine($"ID: `{user.ID}`");
            value.AppendLine($"Profile: <@{user.ID}>");
            return new EmbedField(":information_source: User information", value.ToString(), false);
        }
    }
}