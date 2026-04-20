using EventManager.Events.EventReader;
using EventManager.Events.Services;
using EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Text;

namespace EventManager.EventRepeat
{
    public class EventRepeatModule(EventService eventService,
                                   EventRepeatConfigurationService repeatConfigurationService) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("set-repeat", "Set whether an event should repeat")]
        public async Task SetRepeatAsync(
            [SlashCommandParameter(TypeReaderType = typeof(EventTypeReader), Description = "Event to edit")] int eventId,
            [SlashCommandParameter(Description = "Number of days to move forward on repetition")] int forwardTime)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            var repeatInfo = await repeatConfigurationService.SetRepeatInfoForEventAsync(eventId, forwardTime);
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent(repeatInfo.ToString()));
        }

        [SlashCommand("set-date-generation", "Set automatic date generation for an event")]
        public async Task SetAutomaticDateGeneration(
            [SlashCommandParameter(TypeReaderType = typeof(EventTypeReader), Description = "Event to edit")] int eventId,
            [SlashCommandParameter(Description = "Which day of the week auto generation should start from")] DayOfWeek startFrom,
            [SlashCommandParameter(Name = "days", Description = "Days to generate", TypeReaderType = typeof(DayOfWeekTypeReader))] DayOfWeekFlags dayOfWeekFlags,
            [SlashCommandParameter(MinValue = 1, MaxValue = 4, Description = "Number of weeks to generate for")] int numberOfWeeks)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            var repeatInfo = await repeatConfigurationService.SetAutomaticDateGenerationForEventAsync(eventId, startFrom, dayOfWeekFlags, numberOfWeeks);
            if (repeatInfo == null)
            {
                await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent("There is no repetition configured for this event"));
            }
            else
            {
                await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent(repeatInfo.ToString()));
            }
        }

        [SlashCommand("disable-repeat", "Disable repeat for event")]
        public async Task DisableRepeatAsync(
            [SlashCommandParameter(TypeReaderType = typeof(EventTypeReader), Description = "Event to edit")] int eventId)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            await repeatConfigurationService.DisableRepeatForEvent(eventId);
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent("Repetition has been disabled"));
        }

        [SlashCommand("get-repeat", "Get current repeat information for event")]
        public async Task GetRepeatAsync(
            [SlashCommandParameter(TypeReaderType = typeof(EventTypeReader), Description = "Event to get info for")] int eventId)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            EventRepeatInfo? repeatInfo = await repeatConfigurationService.TryGetRepeatInfoForEvent(eventId);
            if (repeatInfo == null)
            {
                await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent("There is no repetition configured for this event"));
            }
            else
            {
                await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent(repeatInfo.ToString()));
            }
        }

        [SlashCommand("get-all-repeat", "Get repeat information for all events")]
        public async Task GetAllRepeatAsync()
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            var events = await eventService.GetEventsWithRepeatInfoAsync(Context.Interaction.GuildId!.Value);
            StringBuilder builder = new();
            foreach (var ev in events)
            {
                if (ev == null)
                {
                    continue;
                }
                if (ev.RepeatInfo == null)
                {
                    builder.AppendLine($"**{ev.Name}**\r\nThere is no repetition configured for this event");
                }
                else
                {
                    builder.AppendLine(ev.RepeatInfo.ToString());
                }
            }
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent(builder.ToString()));
        }

        [SlashCommand("disable-date-generation", "Disable date generation for event")]
        public async Task DisableDateGenerationAsync(
            [SlashCommandParameter(TypeReaderType = typeof(EventTypeReader), Description = "Event to edit")] int eventId)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            await repeatConfigurationService.DisableAutomaticDateGenerationForEvent(eventId);
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent("Date generation has been disabled"));
        }
    }
}
