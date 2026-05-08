using EventManager.Events.Models;
using EventManager.Events.Services;
using EventManager.GenerateDates;
using EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;

namespace EventManager.EventRepeat
{
    public class EventRepeatService(RestClient client,
                                    EventService eventService,
                                    GenerateDatesService generateDatesService,
                                    ILogger<EventRepeatService> logger)
    {
        public async Task<bool> TryRepeatEventAsync(GuildScheduledEvent? scheduledEvent, DiscordEvent ev)
        {
            if (ev.RepeatInfo == null || !ev.RepeatInfo.Repeat)
            {
                logger.LogInformation("Event {event} will not be repeated", ev.Name);
                ev.Expired = true;
                if (ev.ChannelId != 0)
                {
                    try
                    {
                        await client.DeleteMessageAsync(ev.ChannelId, ev.MessageId);
                    }
                    catch
                    {

                    }
                }
                return false;
            }

            logger.LogInformation("Repeating event {event}", ev.Name);
            string eventname = ev.Name!.Contains(Constants.AutoRepeatPrefix) ? ev.Name : $"{Constants.AutoRepeatPrefix}{ev.Name}";
            DateTime originalStartDateTime = ev.StartDateTime;
            (var newStart, var newEnd) = GetNewStartAndEndTime(ev);

            try
            {
                var newEvent = await client.CreateGuildScheduledEventAsync(ev.GuildId, CreateScheduledEventArguments(scheduledEvent, ev, eventname, newStart, newEnd));
                ev.InviteUrl = ev.InviteUrl!.Replace(ev!.EventId.ToString(), newEvent.Id.ToString());
                ev.Update(newEvent);
                ev.AutomaticallyCreated = true;
                var message = await client.GetMessageAsync(ev.ChannelId, ev.MessageId);
                await message.ModifyAsync(msg => msg.WithContent(ev.InviteUrl));
                if (originalStartDateTime.ToLocalTime() <= DateTime.Now)
                {
                    await GenerateDatesIfConfigured(ev);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception during event repetition");
            }

            return true;
        }

        private async Task GenerateDatesIfConfigured(DiscordEvent ev)
        {
            if (!ev.RepeatInfo!.HasAutomaticDateGeneration)
            {
                return;
            }
            if (ev.RepeatInfo.DelayDateGeneration != null)
            {
                ev.RunDateGenerationOn = DateOnly.FromDateTime(DateTime.Today.AddDays(ev.RepeatInfo.DelayDateGeneration.Value));
            }
            else
            {
                await GenerateDates(ev);
            }
        }

        public async Task GenerateDates(DiscordEvent ev)
        {
            if (ev.RepeatInfo!.HasAutomaticDateGeneration && DayOfWeekFlagsExtensions.TryParse(ev.RepeatInfo!.DaysOfWeekPattern, out DayOfWeekFlags result))
            {
                logger.LogInformation("Start generating dates on repeat for event {event}", ev.Name);
                DateTime nextTarget = ev.StartDateTime.AddDays((7 + (int)ev.RepeatInfo.TargetDayOfWeek! - (int)ev.StartDateTime.DayOfWeek) % 7);
                await generateDatesService.GenerateDatesAsync(ev.ThreadChannelId, nextTarget, nextTarget.AddDays(7 * ev.RepeatInfo.NumberOfWeeksToGenerate!.Value), result);
            }
        }

        private (DateTime, DateTime?) GetNewStartAndEndTime(DiscordEvent ev)
        {
            DateTime newStart = ev.StartDateTime.AddDays(ev.RepeatInfo!.ForwardTime).ToLocalTime();
            DateTime? newEnd = ev.EndDateTime?.AddDays(ev.RepeatInfo.ForwardTime).ToLocalTime();
            if (newStart > DateTime.Now)
            {
                return (newStart, newEnd);
            }
            logger.LogInformation("New starttime {newStart:dd-MM-yyyy HH:mm} is in the past, moving forward.", newStart);
            while (newStart < DateTime.Now)
            {
                newStart = newStart.AddDays(1);
                newEnd = newEnd?.AddDays(1);
            }
            logger.LogInformation("New starttime moved to {newStart:dd-MM-yyyy HH:mm}", newStart);
            return (newStart, newEnd);
        }

        private static GuildScheduledEventProperties CreateScheduledEventArguments(GuildScheduledEvent? scheduledEvent, DiscordEvent ev, string eventname, DateTime newStart, DateTime? newEnd)
        {
            GuildScheduledEventProperties result =
                new(eventname,
                    scheduledEvent?.PrivacyLevel ?? GuildScheduledEventPrivacyLevel.GuildOnly,
                    newStart,
                    ev.EntityType)
                {
                    Description = ev.Description,
                    ScheduledEndTime = newEnd
                };
            if (ev.EntityType == GuildScheduledEventEntityType.External)
            {
                result.Metadata = new(ev.Location!);
            }
            else
            {
                result.ChannelId = ev.EventChannelId;
            }
            return result;
        }

        public async Task HandleDelayedGenerateDates(ulong guildId)
        {
            logger.LogInformation("Running delayed date generation for guild {guild}", guildId);
            var events = await eventService.GetEventsWithGenerateDatesOn(guildId);
            foreach (var ev in events)
            {
                await GenerateDates(ev!);
                await eventService.SetEventGenerateDatesOn(ev, null);
            }
        }
    }
}
