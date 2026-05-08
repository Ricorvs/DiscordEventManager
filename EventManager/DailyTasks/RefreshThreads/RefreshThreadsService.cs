using EventManager.Events.Models;
using EventManager.Events.Services;
using EventManager.GuildConfiguration;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace EventManager.DailyTasks.RefreshThreads
{
    public class RefreshThreadsService(RestClient client,
                                       EventService eventService,
                                       GuildConfigurationService guildConfigurationService,
                                       ILogger<RefreshThreadsService> logger) : IGuildThreadUpdateGatewayHandler
    {
        async ValueTask IGuildThreadUpdateGatewayHandler.HandleAsync(GuildThread arg)
        {
            if (!arg.Metadata.Archived)
            {
                return;
            }
            var ev = await eventService.GetEventFromThreadIdAsync(arg.Id);
            if (await ShouldUnarchiveThreadAsync(ev))
            {
                logger.LogInformation("Unarchiving thread for event {event}", ev!.Name);
                await arg.ModifyAsync(thread => thread.Archived = false);
            }
        }

        private async Task<bool> ShouldUnarchiveThreadAsync(DiscordEvent? ev)
        {
            if (ev == null)
            {
                return false;
            }
            if (!ev.Expired)
            {
                return true;
            }
            var endTime = (ev.EndDateTime ?? ev.StartDateTime);
            if (endTime > DateTime.Now)
            {
                return false;
            }
            var config = await guildConfigurationService.GetGuildConfigurationAsync(ev.GuildId!);
            if (config == null || config.ThreadKeepAliveTime == null)
            {
                return false;
            }
            endTime = endTime.AddDays(config.ThreadKeepAliveTime.Value);
            return DateTime.Now <= endTime;
        }

        public async Task RestoreAllThreadsAsync(RestGuild guild)
        {
            logger.LogInformation("Restoring all threads for guild {guild}", guild.Name);
            var events = await eventService.GetEventsAsync(guild.Id);
            foreach (var ev in events)
            {
                GuildThread? thread = await client.GetChannelAsync(ev!.ThreadChannelId) as GuildThread;
                if (thread != null)
                {
                    await thread.ModifyAsync(thread => thread.Archived = false);
                }
            }
        }
        public async Task ArchiveThreads(RestGuild guild, GuildConfiguration.GuildConfiguration? configuration)
        {
            if (configuration?.EventChannel == null)
            {
                return;
            }
            logger.LogInformation("Archiving threads for guild {guild} in channel {channel}", guild.Name, configuration.EventChannel);
            var threads = await guild.GetActiveThreadsAsync();
            foreach (var thread in threads)
            {
                if (thread.ParentId == configuration.EventChannel)
                {
                    await thread!.ModifyAsync(tr => tr.Archived = true);
                }
            }
        }
    }
}
