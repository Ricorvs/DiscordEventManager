using EventManager.Events.Services;
using EventManager.GuildConfiguration;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using System.Globalization;

namespace EventManager.GenerateDates
{
    public class MessageReactionGatewayHandler(RestClient client,
                                      GuildConfigurationService guildConfigurationService,
                                      EventRegistrationService eventRegistrationService,
                                      EventService eventService,
                                      ILogger<MessageReactionGatewayHandler> logger) : IMessageReactionAddGatewayHandler,
                                                                                       IMessageReactionRemoveGatewayHandler
    {
        private readonly ReactionEmojiProperties _greenSquare = new("🟩");
        private readonly ReactionEmojiProperties _yellowSquare = new("🟨");
        private readonly ReactionEmojiProperties _orangeSquare = new("🟧");
        private readonly ReactionEmojiProperties _pinEmoji = new("📌");

        private readonly Dictionary<string, string> KnownReactions = new()
        {
            { "❌", "Unavailable" },
            { "〽️", "Maybe" },
            { "✅", "Available" },
            { "📌", "Pin" }
        };

        private static async Task<int> GetCount<T>(IAsyncEnumerable<T> values)
        {
            int count = 0;
            await foreach (var item in values)
            {
                count++;
            }
            return count;
        }

        private async Task ProcessReactions(string reaction, ulong guildId, ulong channelId, ulong messageId, RestMessage? message)
        {
            switch (reaction)
            {
                case "❌":
                case "〽️":
                    await ProcessUnavailableReactions(channelId, messageId);
                    break;
                case "✅":
                    await ProcessAvailableReactions(guildId, channelId, messageId);
                    break;
                case "📌":
                    await PinDate(channelId, messageId, message);
                    break;
                default:
                    break;
            }
            return;
        }

        private async Task ProcessUnavailableReactions(ulong channelId, ulong messageId)
        {
            var emotes = await GetCount(client.GetMessageReactionsAsync(channelId, messageId, new ReactionEmojiProperties("❌")));
            int count = (emotes - 1) * 2;
            emotes = await GetCount(client.GetMessageReactionsAsync(channelId, messageId, new ReactionEmojiProperties("〽️")));
            count += emotes - 1;

            (ReactionEmojiProperties emoji, bool active)[] status =
            [
                (_greenSquare, count < 2),
                (_yellowSquare, count > 0 && count < 4),
                (_orangeSquare, count > 2)
            ];
            foreach ((ReactionEmojiProperties emoji, bool active) in status)
            {
                if (active)
                {
                    await client.AddMessageReactionAsync(channelId, messageId, emoji);
                }
                else
                {
                    await client.DeleteCurrentUserMessageReactionAsync(channelId, messageId, emoji);
                }
            }
        }

        private async Task ProcessAvailableReactions(ulong guildId, ulong channelId, ulong messageId)
        {
            var emotes = await GetCount(client.GetMessageReactionsAsync(channelId, messageId, new ReactionEmojiProperties("✅")));
            int available = (emotes - 1);

            var discordEvent = await eventService.GetEventFromThreadIdAsync(channelId);
            if (discordEvent == null)
            {
                return;
            }

            var guildConfiguration = await guildConfigurationService.GetGuildConfigurationAsync(guildId);
            if (guildConfiguration?.PinDateThreshold != null && available >= guildConfiguration.PinDateThreshold)
            {
                await client.AddMessageReactionAsync(channelId, messageId, _pinEmoji);
            }
            else
            {
                await client.DeleteCurrentUserMessageReactionAsync(channelId, messageId, _pinEmoji);
            }
        }

        private async Task PinDate(ulong channelId, ulong messageId, RestMessage? message)
        {
            message ??= await client.GetMessageAsync(channelId, messageId);
            if (message == null)
            {
                return;
            }
            logger.LogInformation("Attempting to pin eventdate associated with channel {channel} based on message {message}", channelId, messageId);
            if (!DateTime.TryParseExact(message.Content, "dddd dd MMMM yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date) ||
                date < DateTime.Today)
            {
                return;
            }
            await eventRegistrationService.SetEventDate(channelId, date);
        }

        public async ValueTask HandleAsync(MessageReactionAddEventArgs arg)
        {
            if (arg.User!.IsBot || !KnownReactions.TryGetValue(arg.Emoji.Name!, out string? emojiName) || arg.GuildId == null)
            {
                return;
            }
            logger.LogInformation("[{guild}] User {user} removed reaction {reaction} from message {message} in channel {channel}", arg.GuildId, arg.User.GlobalName, emojiName, arg.MessageId, arg.ChannelId);

            var app = await client.GetCurrentBotApplicationInformationAsync();
            if (arg.MessageAuthorId != app.Bot!.Id)
            {
                return;
            }
            await ProcessReactions(arg.Emoji.Name!, arg.GuildId.Value, arg.ChannelId, arg.MessageId, null);
        }

        public async ValueTask HandleAsync(MessageReactionRemoveEventArgs arg)
        {
            if (!KnownReactions.TryGetValue(arg.Emoji.Name!, out string? emojiName) || arg.GuildId == null || arg.Emoji.Name == "📌")
            {
                return;
            }
            logger.LogInformation("[{guild}] User {user} removed reaction {reaction} from message {message} in channel {channel}", arg.GuildId, arg.UserId, emojiName, arg.MessageId, arg.ChannelId);
            var app = await client.GetCurrentBotApplicationInformationAsync();
            var message = await client.GetMessageAsync(arg.ChannelId, arg.MessageId);
            if (message.Author.Id != app.Bot!.Id || arg.UserId == app.Bot.Id)
            {
                return;
            }
            await ProcessReactions(arg.Emoji.Name!, arg.GuildId.Value, arg.ChannelId, arg.MessageId, message);
        }
    }
}
