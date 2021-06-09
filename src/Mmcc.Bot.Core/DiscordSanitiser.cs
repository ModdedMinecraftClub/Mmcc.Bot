using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Remora.Discord.API.Abstractions.Objects;

namespace Mmcc.Bot.Core
{
    /// <summary>
    /// Sanitises <see cref="IMessage"/>'s content so that it is compatible with polychat2's formatting.
    /// </summary>
    public static class DiscordSanitiser
    {
        // matches mentions with a group containing the ID;
        private const string UsernameMentionRegex = "<@([0-9]+)>";
        private const string NicknameMentionRegex = "<@!([0-9]+)>";
        private const string ChannelMentionRegex = "<#([0-9]+)>";
        private const string RoleMentionRegex = "<@&([0-9]+)>";

        // inspired by: https://www.regextester.com/106421
        // standard emojis, i.e. Unicode ones;
        private const string AllStandardEmojiRegex =
            "(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])";

        // matches all custom emoji with a group containing the emoji name;
        private const string CustomEmojiRegex = "<:(.*):[0-9]+>";

        /// <summary>
        /// Sanitises a Discord <see cref="string"/> (usually <see cref="IMessage"/>'s content) so that it is
        /// compatible with polychat2's formatting.
        /// </summary>
        /// <param name="s"><see cref="string"/> to sanitise</param>
        /// <param name="userMentionsToMatch">User mentions to match to the textual mentions.</param>
        /// <param name="channelMentionsToMatch">Channel mentions to match the textual mentions.</param>
        /// <returns>Sanitised <see cref="string"/>.</returns>
        public static string Sanitise(
            string s,
            IEnumerable<IUserMention>? userMentionsToMatch = null,
            IEnumerable<IChannelMention>? channelMentionsToMatch = null
        )
        {
            if (userMentionsToMatch is not null)
            {
                // ReSharper disable once ConvertToLocalFunction
                MatchEvaluator onMatch = match =>
                {
                    var parseSuccessful = ulong.TryParse(match.Groups[1].Value, out var id);

                    if (!parseSuccessful) return match.ToString();
                    
                    var matchingUsername = userMentionsToMatch.FirstOrDefault(u => u.ID.Value == id);
                    return matchingUsername is not null
                        ? $"@{matchingUsername.Username}#{matchingUsername.Discriminator}"
                        : match.ToString();
                };
                
                // replace username mentions;
                s = Regex.Replace(s, UsernameMentionRegex, onMatch);
                // replace nickname mentions;
                s = Regex.Replace(s, NicknameMentionRegex, onMatch);
            }

            if (channelMentionsToMatch is not null)
            {
                s = Regex.Replace(s, ChannelMentionRegex, match =>
                {
                    var parseSuccessful = ulong.TryParse(match.Groups[1].Value, out var id);

                    if (!parseSuccessful) return match.ToString();

                    var matchingChannel = channelMentionsToMatch.FirstOrDefault(c => c.ID.Value == id);
                    return matchingChannel is not null
                        ? $"#{matchingChannel.Name}"
                        : match.ToString();
                });
            }

            s = Regex.Replace(s, RoleMentionRegex, _ => "@Discord_Role");
            s = Regex.Replace(s, AllStandardEmojiRegex, match =>
            {
                var emojiUnicodeChar = match.Value;
                var shortname = EmojiOne.EmojiOne.ToShort(emojiUnicodeChar);

                if (shortname is null) return ":unknown_emoji:";

                var asciiOrShortname = EmojiOne.EmojiOne.ShortnameToAscii(shortname);
                return asciiOrShortname.Replace("‍", " ");
            });
            
            s = Regex.Replace(s, CustomEmojiRegex, match => $":{match.Groups[1].Value}:");
            return s.Replace("️", "");
        }
    }
}