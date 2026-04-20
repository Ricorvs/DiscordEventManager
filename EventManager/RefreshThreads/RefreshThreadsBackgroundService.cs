using EventManager.GuildConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Rest;

namespace EventManager.RefreshThreads
{
    public class RefreshThreadsBackgroundService(RestClient client,
                                                 GuildConfigurationService guildConfigurationService,
                                                 ILogger<RefreshThreadsBackgroundService> logger) : BackgroundService
    {
        private async Task ArchiveThreads(RestGuild guild, ulong channelId)
        {
            logger.LogInformation("Archiving threads for guild {guild} in channel {channel}", guild.Name, channelId);
            var threads = await guild.GetActiveThreadsAsync();
            foreach (var thread in threads)
            {
                if (thread.ParentId == channelId)
                {
                    await thread!.ModifyAsync(tr => tr.Archived = true);
                }
            }
        }

        private async void TimerTriggered(object? state)
        {
            RestGuild guild = (RestGuild)state!;
            var config = await guildConfigurationService.GetGuildConfigurationAsync(guild.Id);

            if (config?.EventChannel != null)
            {
                await ArchiveThreads(guild, config.EventChannel.Value);
            }

            UpdateTimerTrigger(guild.Id, config);
        }

        private static TimeSpan GetNextTriggerFromConfig(GuildConfiguration.GuildConfiguration? config)
        {
            TimeSpan nextTrigger = TimeSpan.FromDays(1);

            if (config?.ThreadRefreshTime != null)
            {
                DateTime target = new DateTime(DateTime.Today.Ticks)
                                    .Add(config.ThreadRefreshTime.Value.ToTimeSpan());
                nextTrigger = target < DateTime.Now
                            ? target.AddDays(1) - DateTime.Now
                            : target - DateTime.Now;
            }
            return nextTrigger;
        }

        public void UpdateTimerTrigger(ulong guildId, GuildConfiguration.GuildConfiguration? config)
        {
            Timer timer = _timers[guildId];
            var nextTrigger = GetNextTriggerFromConfig(config);
            logger.LogInformation("Thread refresh timer for guild {guild} will trigger again in {timeremaining}", guildId, nextTrigger);
            timer.Change(nextTrigger, Timeout.InfiniteTimeSpan);
        }

        private readonly Dictionary<ulong, Timer> _timers = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var guilds = client.GetCurrentUserGuildsAsync();
            await foreach (var guild in guilds)
            {
                _timers.Add(guild.Id, new(TimerTriggered, guild, 0, Timeout.Infinite));
            }
        }
    }
}
