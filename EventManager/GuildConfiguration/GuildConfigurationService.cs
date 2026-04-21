using EventManager.DbContext;
using Microsoft.EntityFrameworkCore;

namespace EventManager.GuildConfiguration
{
    public class GuildConfigurationService(IDbContextFactory<EventManagerDbContext> dbContextFactory)
    {
        public async Task<GuildConfiguration?> GetGuildConfigurationAsync(ulong guildId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.GuildConfiguration.FirstOrDefaultAsync(config => config.GuildId == guildId);
        }

        public async Task SetGuildEventChannelAsync(ulong guildId, ulong channelId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existingConfig = await dbContext.GuildConfiguration.FirstOrDefaultAsync(config => config.GuildId == guildId);

            if (existingConfig == null)
            {
                existingConfig ??= new() { GuildId = guildId, EventChannel = channelId };
                dbContext.GuildConfiguration.Add(existingConfig);
            }
            else
            {
                existingConfig.EventChannel = channelId;
                dbContext.GuildConfiguration.Attach(existingConfig).State = EntityState.Modified;
            }
            await dbContext.SaveChangesAsync();
        }

        public async Task<GuildConfiguration> SetGuildThreadRefreshTimeAsync(ulong guildId, TimeOnly? refreshTime)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existingConfig = await dbContext.GuildConfiguration.FirstOrDefaultAsync(config => config.GuildId == guildId);

            if (existingConfig == null)
            {
                existingConfig ??= new() { GuildId = guildId, ThreadRefreshTime = refreshTime };
                dbContext.GuildConfiguration.Add(existingConfig);
            }
            else
            {
                existingConfig.ThreadRefreshTime = refreshTime;
                dbContext.GuildConfiguration.Attach(existingConfig).State = EntityState.Modified;
            }
            await dbContext.SaveChangesAsync();
            return existingConfig;
        }
        public async Task SetGuildThreadKeepAliveTimeAsync(ulong guildId, int keepAliveTime)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existingConfig = await dbContext.GuildConfiguration.FirstOrDefaultAsync(config => config.GuildId == guildId);

            if (existingConfig == null)
            {
                existingConfig ??= new() { GuildId = guildId, ThreadKeepAliveTime = keepAliveTime };
                dbContext.GuildConfiguration.Add(existingConfig);
            }
            else
            {
                existingConfig.ThreadKeepAliveTime = keepAliveTime;
                dbContext.GuildConfiguration.Attach(existingConfig).State = EntityState.Modified;
            }
            await dbContext.SaveChangesAsync();
        }

        public async Task<ulong?> GetEventChannelForGuild(ulong guildId)
        {
            var config = await GetGuildConfigurationAsync(guildId);
            return config?.EventChannel;
        }
    }
}
