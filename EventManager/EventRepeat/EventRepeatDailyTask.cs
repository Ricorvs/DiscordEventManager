using EventManager.DailyTasks;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Rest;

namespace EventManager.EventRepeat
{
    public class EventRepeatDailyTask(IServiceProvider provider) : IDailyGuildTask
    {
        async Task IDailyGuildTask.Execute(RestGuild guild, GuildConfiguration.GuildConfiguration? configuration)
        {
            var eventRepeatService = provider.GetRequiredService<EventRepeatService>();
            await eventRepeatService.HandleDelayedGenerateDates(guild.Id);
        }
    }
}
