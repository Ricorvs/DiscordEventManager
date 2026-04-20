using EventManager.Events.EventReader;
using EventManager.Events.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Text;

namespace EventManager.Events.Modules
{
    public class EventInterestModule(RestClient client, EventService eventService) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("get-interested-for-event", "Get the users which are interested in the event")]
        public async Task GetInterestedAsync(
            [SlashCommandParameter(TypeReaderType = typeof(EventTypeReader), Description = "Event to get interested for")] int eventId)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

            var ev = await eventService.GetEventWithUsersAsync(eventId);
            if (ev == null)
            {
                await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent("Event not found"));
                return;
            }
            StringBuilder builder = new();
            builder.AppendLine(ev.Name);
            foreach (var user in ev.InterestedUsers!)
            {
                var discordUser = await client.GetUserAsync(user.UserId);
                if (discordUser == null)
                {
                    builder.AppendLine(user.UserId.ToString());
                }
                else
                {
                    builder.AppendLine(discordUser.GlobalName);
                }
            }
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent(builder.ToString()));
        }

        [SlashCommand("get-user-interested", "Get events the user is interested in")]
        public async Task GetInterestedForUser([SlashCommandParameter(Description = "User to get events for")] User? user = null)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            user ??= Context.Interaction.User;
            var events = await eventService.GetEventsAsync(Context.Interaction.GuildId!.Value, user.Id);

            StringBuilder builder = new();
            builder.AppendLine($"User {user.GlobalName} is interested in these events");
            foreach (var ev in events)
            {
                builder.AppendLine(ev!.Name);
            }

            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent(builder.ToString()));
        }
    }
}
