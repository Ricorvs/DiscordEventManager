using NetCord.Rest;

namespace EventManager.DailyTasks
{
    public interface IDailyGuildTask
    {
        public Task Execute(RestGuild guild, GuildConfiguration.GuildConfiguration? configuration);
    }
}
