using EventManager.DbContext;
using EventManager.Events.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Events.Services
{
    public class EventService(IDbContextFactory<EventManagerDbContext> dbContextFactory)
    {
        public async Task<DiscordEvent?> GetEventAsync(ulong eventId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Events
                .Where(ev => ev.EventId == eventId)
                .Include(ev => ev.RepeatInfo)
                .FirstOrDefaultAsync();
        }

        public async Task<DiscordEvent?> GetEventWithUsersAsync(int eventId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Events
                .Where(ev => ev.Id == eventId)
                .Include(ev => ev.InterestedUsers)
                .FirstOrDefaultAsync();
        }
        public async Task<DiscordEvent?> GetEventFromThreadIdAsync(ulong threadId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Events
                .Where(ev => ev.ThreadChannelId == threadId)
                .Include(ev => ev.RepeatInfo)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DiscordEvent?>> GetEventsAsync(ulong guildId, ulong? userId = null, bool ignoreAutomaticallyCreated = false)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var events = dbContext.Events.Where(ev => ev.GuildId == guildId && !ev.Expired && (!ignoreAutomaticallyCreated || !ev.AutomaticallyCreated));
            if (userId != null)
            {
                events = events.Where(ev => ev.InterestedUsers != null && ev.InterestedUsers.Any(interest => interest.UserId == userId));
            }
            return await events.ToArrayAsync();
        }

        public async Task<IEnumerable<DiscordEvent?>> GetEventsWithRepeatInfoAsync(ulong guildId, ulong? userId = null, bool ignoreAutomaticallyCreated = false)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var events = dbContext.Events
                .Include(ev => ev.RepeatInfo)
                .Where(ev => ev.GuildId == guildId && !ev.Expired && (!ignoreAutomaticallyCreated || !ev.AutomaticallyCreated));
            if (userId != null)
            {
                events = events.Where(ev => ev.InterestedUsers != null && ev.InterestedUsers.Any(interest => interest.UserId == userId));
            }
            return await events.ToArrayAsync();
        }

        public async Task<DiscordEvent?> SaveEvent(DiscordEvent? ev)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            if (ev == null)
            {
                return null;
            }
            if (ev.Id == null)
            {
                dbContext.Events.Add(ev);
            }
            else
            {
                dbContext.Events.Attach(ev).State = EntityState.Modified;
            }
            await dbContext.SaveChangesAsync();
            return ev;
        }
    }
}
