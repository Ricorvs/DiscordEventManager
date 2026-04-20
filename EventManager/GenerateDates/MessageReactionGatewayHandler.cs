using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace EventManager.GenerateDates
{
    public class MessageReactionGatewayHandler(RestClient client) : IMessageReactionAddGatewayHandler,
                                                                    IMessageReactionRemoveEmojiGatewayHandler,
                                                                    IMessageReactionRemoveGatewayHandler
    {
        private ReactionEmojiProperties _greenSquare = new("🟩");
        private ReactionEmojiProperties _yellowSquare = new("🟨");
        private ReactionEmojiProperties _orangeSquare = new("🟧");
        public async ValueTask HandleAsync(MessageReactionAddEventArgs arg)
        {
            if (arg.User!.IsBot || arg.Emoji is not { Name: "❌" or "〽️" })
            {
                return;
            }
            var app = await client.GetCurrentBotApplicationInformationAsync();
            if (arg.MessageAuthorId != app.Bot!.Id)
            {
                return;
            }
            await ProcessReactions(arg.ChannelId, arg.MessageId);
        }

        private async Task<int> GetCount<T>(IAsyncEnumerable<T> values)
        {
            int count = 0;
            await foreach (var item in values)
            {
                count++;
            }
            return count;
        }

        private async Task ProcessReactions(ulong channelId, ulong messageId)
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

        public async ValueTask HandleAsync(MessageReactionRemoveEmojiEventArgs arg)
        {
            if (arg.Emoji is not { Name: "❌" or "〽️" })
            {
                return;
            }

            var app = await client.GetCurrentBotApplicationInformationAsync();
            var message = await client.GetMessageAsync(arg.ChannelId, arg.MessageId);
            if (message.Author.Id != app.Bot!.Id)
            {
                return;
            }

            await ProcessReactions(arg.ChannelId, arg.MessageId);
        }

        public async ValueTask HandleAsync(MessageReactionRemoveEventArgs arg)
        {
            if (arg.Emoji is not { Name: "❌" or "〽️" })
            {
                return;
            }
            var app = await client.GetCurrentBotApplicationInformationAsync();
            var message = await client.GetMessageAsync(arg.ChannelId, arg.MessageId);
            if (message.Author.Id != app.Bot!.Id)
            {
                return;
            }
            await ProcessReactions(arg.ChannelId, arg.MessageId);
        }
    }
}
