using EventManager.DbContext;
using EventManager.EventRepeat;
using EventManager.Events.Models;
using EventManager.GuildConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;

namespace EventManager.Events.Services
{
    public class EventRegistrationService(RestClient restclient,
                                          EventService eventService,
                                          EventRepeatService repeatService,
                                          GuildConfigurationService guildConfiguration,
                                          IDbContextFactory<EventManagerDbContext> dbContextFactory,
                                          ILogger<EventRegistrationService> logger)
    {
        private TaskQueue TaskQueue { get; } = new TaskQueue();

        public async Task<DiscordEvent> HandleEventAdded(GuildScheduledEvent arg)
        {
            logger.LogInformation("Event '{event}' added", arg.Name);
            return await TaskQueue.Enqueue(() => HandleEventAddedOrChanged(arg, true));
        }

        public async Task<DiscordEvent> HandleEventChanged(GuildScheduledEvent arg)
        {
            logger.LogInformation("Event '{event}' changed", arg.Name);
            return await TaskQueue.Enqueue(() => HandleEventAddedOrChanged(arg));
        }

        public async Task<DiscordEvent?> HandleEventStarted(GuildScheduledEvent arg)
        {
            logger.LogInformation("Event '{event}' started", arg.Name);
            return await TaskQueue.Enqueue(() => InternalHandleEventStarted(arg));
        }

        public async Task<DiscordEvent?> HandleEventCompleted(GuildScheduledEvent arg)
        {
            logger.LogInformation("Event '{event}' completed", arg.Name);
            return await TaskQueue.Enqueue(() => InternalHandleEventCompleted(null, arg));
        }
        public async Task<DiscordEvent?> HandleEventCompleted(DiscordEvent arg)
        {
            logger.LogInformation("Event '{event}' completed", arg.Name);
            return await TaskQueue.Enqueue(() => InternalHandleEventCompleted(arg, null));
        }

        public async Task HandleEventUserInterested(ulong eventId, ulong userId)
        {
            logger.LogInformation("User '{user}' interested in event '{event}'", userId, eventId);
            await TaskQueue.Enqueue(() => HandleEventUserChanged(eventId, userId, true));
        }

        public async Task HandleEventUserNotInterested(ulong eventId, ulong userId)
        {
            logger.LogInformation("User '{user}' no longer interested in event '{event}'", userId, eventId);
            await TaskQueue.Enqueue(() => HandleEventUserChanged(eventId, userId, false));
        }

        public async Task<bool> SetEventDate(ulong channelId, DateTime date)
        {
            var discordEvent = await eventService.GetEventFromThreadIdAsync(channelId);
            if (discordEvent == null)
            {
                logger.LogInformation("No event associated with channel {channel}, could not set eventdate", channelId);
                return false;
            }
            if (discordEvent.Expired)
            {
                logger.LogInformation("Event {event} has already been completed, could not set eventdate", discordEvent.Name);
                return false;
            }

            DateTime newStartDateTime = new(date.Year, date.Month, date.Day, discordEvent.StartDateTime.Hour, discordEvent.StartDateTime.Minute, discordEvent.StartDateTime.Second);
            DateTime? newEndDateTime = null;
            if (discordEvent.EndDateTime != null)
            {
                newEndDateTime = newStartDateTime.Add(discordEvent.EndDateTime.Value - discordEvent.StartDateTime);
            }
            logger.LogInformation("Setting eventdate for event {event} to {startdate}-{enddate}", discordEvent.Name, newStartDateTime, newEndDateTime);
            await restclient.ModifyGuildScheduledEventAsync(discordEvent.GuildId, discordEvent.EventId, ev =>
            {
                ev.ScheduledStartTime = newStartDateTime.ToLocalTime();
                ev.ScheduledEndTime = newEndDateTime?.ToLocalTime();
            });
            return true;
        }

        private async Task<(DiscordEvent, GuildThread)> HandleRegisterEvent(GuildScheduledEvent arg)
        {
            DiscordEvent discordEvent = new(arg);
            var channelId = await guildConfiguration.GetEventChannelForGuild(arg.GuildId);
            discordEvent.ChannelId = channelId!.Value;
            discordEvent.InviteUrl = $"https://discord.com/events/{arg.GuildId}/{discordEvent.EventId}";
            var message = await restclient.SendMessageAsync(channelId!.Value, new() { Content = discordEvent.InviteUrl });
            discordEvent.MessageId = message.Id;
            var threadChannel = await restclient.CreateGuildThreadAsync(channelId!.Value, message.Id, new(arg.Name));
            discordEvent.ThreadChannelId = threadChannel.Id;
            return (discordEvent, threadChannel);
        }

        private async Task<(DiscordEvent discordEvent, GuildThread threadChannel)> GetEventInfoForScheduledEvent(GuildScheduledEvent scheduledEvent)
        {
            GuildThread? threadChannel = null;
            DiscordEvent? discordEvent = await eventService.GetEventAsync(scheduledEvent.Id);

            if (discordEvent == null)
            {
                (discordEvent, threadChannel) = await HandleRegisterEvent(scheduledEvent);
            }
            threadChannel ??= await restclient.GetChannelAsync(discordEvent.ThreadChannelId) as GuildThread;
            if (threadChannel == null)
            {
                throw new Exception("Could not find thread channel");
            }
            return (discordEvent, threadChannel);
        }

        private async Task HandleEventChanged(GuildScheduledEvent arg, DiscordEvent discordEvent, GuildThread threadChannel)
        {
            await HandleDateChanged(arg, discordEvent, threadChannel);
            await HandleRemoveAutoRepeatTag(arg, discordEvent, threadChannel);
            await HandleEventNameChanged(arg, threadChannel);
        }

        private async Task HandleEventNameChanged(GuildScheduledEvent arg, GuildThread threadChannel)
        {
            if (threadChannel.Name != arg.Name)
            {
                logger.LogInformation("Changing channel name from '{oldname}' to '{newname}'", threadChannel.Name, arg.Name);
                await threadChannel!.ModifyAsync(channel => channel.WithName(arg.Name));
            }
        }

        private async Task HandleRemoveAutoRepeatTag(GuildScheduledEvent arg, DiscordEvent discordEvent, GuildThread threadChannel)
        {
            if (discordEvent.AutomaticallyCreated)
            {
                logger.LogInformation("Removing '{autorepeattag}' from eventname", Constants.AutoRepeatPrefix);
                discordEvent.Name = arg.Name.Replace(Constants.AutoRepeatPrefix, string.Empty);
                discordEvent.AutomaticallyCreated = false;
                if (arg.Name != discordEvent.Name)
                {
                    await restclient.ModifyGuildScheduledEventAsync(arg.GuildId, arg.Id, ev => ev.WithName(discordEvent.Name));
                }
            }
        }

        private async Task HandleDateChanged(GuildScheduledEvent arg, DiscordEvent discordEvent, GuildThread threadChannel)
        {
            if (arg.ScheduledStartTime.Date != discordEvent.StartDateTime.Date || discordEvent.AutomaticallyCreated)
            {
                logger.LogInformation("Event date changed to {date:dddd dd MMMM yyyy}", arg.ScheduledStartTime);
                await threadChannel.SendMessageAsync(new() { Content = $"Event date changed to {arg.ScheduledStartTime:dddd dd MMMM yyyy}!" });
            }
        }

        private async Task UpdateThreadLinkInDescription(GuildScheduledEvent arg, DiscordEvent discordEvent)
        {
            string threadLink = $"<#{discordEvent.ThreadChannelId}>";
            if (!discordEvent.Description!.Contains(threadLink))
            {
                discordEvent.Description += Environment.NewLine + threadLink;
                await restclient.ModifyGuildScheduledEventAsync(arg.GuildId, arg.Id, ev => ev.WithDescription(discordEvent.Description));
            }
        }

        private async Task<DiscordEvent> HandleEventAddedOrChanged(GuildScheduledEvent arg, bool create = false)
        {
            (DiscordEvent discordEvent, GuildThread threadChannel) = await GetEventInfoForScheduledEvent(arg);
            if (!create)
            {
                await HandleEventChanged(arg, discordEvent, threadChannel);
            }
            discordEvent.Update(arg);
            await UpdateThreadLinkInDescription(arg, discordEvent);
            await eventService.SaveEvent(discordEvent);
            return discordEvent;
        }

        private async Task<DiscordEvent?> InternalHandleEventStarted(GuildScheduledEvent arg)
        {
            DiscordEvent? discordEvent = await eventService.GetEventAsync(arg.Id);
            if (discordEvent == null)
            {
                return null;
            }
            if (discordEvent.AutomaticallyCreated)
            {
                logger.LogInformation("Automatically ending event {event} because it was automatically created", arg.Name);
                await restclient.ModifyGuildScheduledEventAsync(arg.GuildId, arg.Id, ev => ev.WithStatus(GuildScheduledEventStatus.Completed));
            }
            return discordEvent;
        }

        private async Task<DiscordEvent?> InternalHandleEventCompleted(DiscordEvent? discordEvent, GuildScheduledEvent? arg)
        {
            discordEvent ??= await eventService.GetEventAsync(arg!.Id);
            if (discordEvent == null)
            {
                return null;
            }
            await repeatService.TryRepeatEventAsync(arg, discordEvent);
            await eventService.SaveEvent(discordEvent);
            return discordEvent;
        }

        #region userinterest
        public async Task HandleEventUserChanged(ulong eventId, ulong userId, bool add)
        {
            var ev = await eventService.GetEventAsync(eventId);
            if (ev == null)
            {
                return;
            }
            GuildThread? thread = await restclient.GetChannelAsync(ev.ThreadChannelId) as GuildThread;
            if (thread == null)
            {
                return;
            }
            if (add)
            {
                await thread.AddUserAsync(userId);
                await SetUserInterested(ev, userId);
            }
            else
            {
                await thread.DeleteUserAsync(userId);
                await SetUserNotInterested(ev, userId);
            }
        }


        private async Task SetUserInterested(DiscordEvent ev, ulong userId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            if (await dbContext.UserEventInterest.AnyAsync(interest => interest.EventId == ev.Id && interest.UserId == userId))
            {
                return;
            }
            await dbContext.UserEventInterest.AddAsync(new() { EventId = ev.Id, UserId = userId });
            await dbContext.SaveChangesAsync();
        }

        private async Task SetUserNotInterested(DiscordEvent ev, ulong userId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var interest = await dbContext.UserEventInterest.FirstOrDefaultAsync(e => e.UserId == userId && e.EventId == ev.Id);
            if (interest != null)
            {
                dbContext.UserEventInterest.Remove(interest);
            }
            await dbContext.SaveChangesAsync();
        }
        #endregion
    }
}
