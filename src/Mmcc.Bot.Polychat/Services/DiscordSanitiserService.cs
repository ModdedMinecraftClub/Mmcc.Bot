using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mmcc.Bot.Polychat.Abstractions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;

namespace Mmcc.Bot.Polychat.Services
{
    /// <summary>
    /// Service to sanitise the content of Discord <see cref="IMessage"/>s so that they can be forwarded
    /// to polychat2 clients in a format that they support.
    /// </summary>
    public interface IDiscordSanitiserService
    {
        /// <summary>
        /// Sanitises an <see cref="IMessage"/>'s content so that it can be forwarded to polychat2's clients
        /// in a format that they support.
        /// </summary>
        /// <param name="message">Message which content is to be sanitised.</param>
        /// <returns><see cref="string"/> representing sanitised content of <see cref="IMessage"/>.</returns>
        Task<string> SanitiseMessageContent(IMessage message);
    }
    
    /// <inheritdoc />
    public class DiscordSanitiserService : IDiscordSanitiserService
    {
        private readonly IDiscordRestGuildAPI _guildApi;

        // matches mentions with a group containing the ID;
        private const string UsernameMentionRegex = "<@([0-9]+)>";
        private const string NicknameMentionRegex = "<@!([0-9]+)>";
        private const string FallbackUsernameWithAt = "@unknown_discord_user";
        
        private const string ChannelMentionRegex = "<#([0-9]+)>";
        private const string FallbackChannelName = "unknown_discord_channel";
        private const string ErrorChannelNameWithHashtag = "#{error_getting_channel_name}";
        
        private const string RoleMentionRegex = "<@&([0-9]+)>";
        private const string FallbackRoleName = "unknown_discord_role";
        private const string ErrorRoleNameWithAt = "@{error_getting_role_name}";

        // standard emojis, i.e. Unicode ones;
        // inspired by: https://www.regextester.com/106421 ;
        private const string StandardEmojiRegex =
            "(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])";
        private const string EmojiFallbackWithColons = ":unknown_emoji:";
        
        // matches all custom emoji with a group containing the emoji name;
        private const string CustomEmojiRegex = "<:(.*):[0-9]+>";
        
        // matches valid minecraft color / format codes, according to https://www.digminecraft.com/lists/color_list_pc.php
        private const string MinecraftCodeRegex = "§[0-9a-rA-R]";

        /// <summary>
        /// Instantiates a new instance of <see cref="DiscordSanitiserService"/>.
        /// </summary>
        /// <param name="guildApi">The Guild API.</param>
        public DiscordSanitiserService(IDiscordRestGuildAPI guildApi)
        {
            _guildApi = guildApi;
        }

        /// <inheritdoc />
        public async Task<string> SanitiseMessageContent(IMessage message)
        {
            var s = message.Content;
            
            s = SanitiseUsernameAndNicknameMentions(s, message.Mentions);
            s = await SanitiseChannelMentions(s, message.GuildID);
            s = await SanitiseRoleMentions(s, message.GuildID);
            s = SanitiseStandardEmoji(s);
            s = SanitiseCustomEmoji(s);
            s = SanitiseMinecraftFormatting(s);
            s = s.Replace("️", " ");

            var chatString = new PolychatChatMessageString(s);
            return chatString.ToSanitisedString();
        }
        
        private string SanitiseUsernameAndNicknameMentions(string s, IEnumerable<IUserMention> userMentions)
        {
            // ReSharper disable once ConvertToLocalFunction
            MatchEvaluator onUsernameOrNicknameMatch = match =>
            {
                var parseSuccessful = ulong.TryParse(match.Groups[1].Value, out var id);

                if (!parseSuccessful)
                {
                    return FallbackUsernameWithAt;
                }
                    
                var matchingUsername = userMentions.FirstOrDefault(u => u.ID.Value == id);
                
                return matchingUsername is not null
                    ? $"@{matchingUsername.Username}#{matchingUsername.Discriminator}"
                    : FallbackUsernameWithAt;
            };
            // replace username mentions;
            s = Regex.Replace(s, UsernameMentionRegex, onUsernameOrNicknameMatch);
            // replace nickname mentions;
            s = Regex.Replace(s, NicknameMentionRegex, onUsernameOrNicknameMatch);
            
            return s;
        }
        
        private async Task<string> SanitiseChannelMentions(string s, Optional<Snowflake> guildId)
        {
            if (!guildId.HasValue)
            {
                return s;
            }

            var getGuildChannelsRes = await _guildApi.GetGuildChannelsAsync(guildId.Value);
            var sanitised = getGuildChannelsRes.IsSuccess
                ? Regex.Replace(s, ChannelMentionRegex, match =>
                {
                    var parseSuccessful = ulong.TryParse(match.Groups[1].Value, out var id);

                    if (!parseSuccessful)
                    {
                        return $"#{FallbackChannelName}";
                    }

                    var matchingChannel = getGuildChannelsRes.Entity
                        .FirstOrDefault(c => c.ID.Value == id);
                    var matchingChannelName = matchingChannel switch
                    {
                        {Name: {HasValue: true}} => matchingChannel.Name.Value,
                        
                        _ => FallbackChannelName
                    };
                    
                    return $"#{matchingChannelName}";
                })
                : Regex.Replace(s, ChannelMentionRegex, ErrorChannelNameWithHashtag);
            
            return sanitised;
        }

        private async Task<string> SanitiseRoleMentions(string s, Optional<Snowflake> guildId)
        {
            if (!guildId.HasValue)
            {
                return s;
            }

            var getGuildRolesRes = await _guildApi.GetGuildRolesAsync(guildId.Value);
            var sanitised = getGuildRolesRes.IsSuccess
                ? Regex.Replace(s, RoleMentionRegex, match =>
                {
                    var parseSuccessful = ulong.TryParse(match.Groups[1].Value, out var id);

                    if (!parseSuccessful)
                    {
                        return $"@{FallbackRoleName}";
                    }

                    var matchingRoleName = getGuildRolesRes.Entity
                        .FirstOrDefault(r => r.ID.Value == id)
                        ?.Name ?? FallbackRoleName;

                    return $"@{matchingRoleName}";
                })
                : Regex.Replace(s, RoleMentionRegex, ErrorRoleNameWithAt);

            return sanitised;
        }

        private string SanitiseStandardEmoji(string s) =>
            Regex.Replace(s, StandardEmojiRegex, match =>
            {
                var emojiUnicodeChar = match.Value;
                var shortname = EmojiOne.EmojiOne.ToShort(emojiUnicodeChar);

                if (shortname is null)
                {
                    return EmojiFallbackWithColons;
                }

                var asciiOrShortname = EmojiOne.EmojiOne.ShortnameToAscii(shortname);
                
                return asciiOrShortname.Replace("‍", " ");
            });

        private string SanitiseCustomEmoji(string s) =>
            Regex.Replace(s, CustomEmojiRegex, match => $":{match.Groups[1].Value}:");
        
        private string SanitiseMinecraftFormatting(string s) =>
            Regex.Replace(s, MinecraftCodeRegex, "");
    }
}