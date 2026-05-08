using Microsoft.Extensions.DependencyInjection;
using NetCord.Rest;

namespace EventManager.DailyTasks.RefreshThreads
{
    public class RefreshThreadsDailyTask(IServiceProvider provider) : IDailyGuildTask
    {
        async Task IDailyGuildTask.Execute(RestGuild guild, GuildConfiguration.GuildConfiguration? configuration)
        {
            var refreshThreadsService = provider.GetRequiredService<RefreshThreadsService>();
            await refreshThreadsService.ArchiveThreads(guild, configuration);
        }
    }
}
