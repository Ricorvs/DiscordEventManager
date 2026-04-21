using EventManager.Events.Services;
using EventManager.RefreshThreads;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Rest;

namespace EventManager.Startup
{
    public class StartupService(RestClient client,
                                EventService eventService,
                                EventRegistrationService eventRegistrationService,
                                RefreshThreadsService refreshThreadsService) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var guilds = client.GetCurrentUserGuildsAsync();
            await foreach (var guild in guilds)
            {
                IReadOnlyList<GuildScheduledEvent> events = await ProcessGuildEvents(guild, cancellationToken);
                await ProcessKnownEvents(guild, events);
            }
        }

        private async Task ProcessKnownEvents(RestGuild guild, IReadOnlyList<NetCord.GuildScheduledEvent> events)
        {
            var knownEvents = await eventService.GetEventsWithRepeatInfoAsync(guild.Id);
            foreach (var knownEvent in knownEvents)
            {
                if (knownEvent == null || events.Any(ev => ev.Id == knownEvent.EventId))
                {
                    continue;
                }
                await eventRegistrationService.HandleEventCompleted(knownEvent);
            }
            await refreshThreadsService.RestoreAllThreadsAsync(guild);
        }

        private async Task<IReadOnlyList<GuildScheduledEvent>> ProcessGuildEvents(RestGuild guild, CancellationToken cancellationToken)
        {
            var events = await client.GetGuildScheduledEventsAsync(guild.Id, cancellationToken: cancellationToken);

            foreach (var ev in events)
            {
                var discordEvent = await eventRegistrationService.HandleEventChanged(ev);
                var users = ev.GetUsersAsync();

                await foreach (var user in users)
                {
                    await eventRegistrationService.HandleEventUserInterested(ev.Id, user.User.Id);
                }
            }

            return events;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
