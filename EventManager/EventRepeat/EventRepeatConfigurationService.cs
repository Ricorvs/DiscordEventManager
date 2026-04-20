using EventManager.DbContext;
using EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader;
using Microsoft.EntityFrameworkCore;

namespace EventManager.EventRepeat
{
    public class EventRepeatConfigurationService(IDbContextFactory<EventManagerDbContext> dbContextFactory)
    {
        public async Task<EventRepeatInfo?> TryGetRepeatInfoForEvent(int eventId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.RepeatInfo.Include(repeat => repeat.Event).FirstOrDefaultAsync(repeat => repeat.EventId == eventId);
        }

        public async Task DisableRepeatForEvent(int eventId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            await dbContext.RepeatInfo
                .Where(repeat => repeat.EventId == eventId)
                .ExecuteUpdateAsync(repeat => repeat.SetProperty(r => r.Repeat, false));
            await dbContext.SaveChangesAsync();
        }

        public async Task<EventRepeatInfo> SetRepeatInfoForEventAsync(int eventId, int forwardTime)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existing = await dbContext.RepeatInfo
                .Include(ev => ev.Event)
                .FirstOrDefaultAsync(ev => ev.EventId == eventId);
            if (existing == null)
            {
                existing = new() { EventId = eventId, Repeat = true, ForwardTime = forwardTime };
                await dbContext.RepeatInfo.AddAsync(existing);
            }
            else
            {
                existing.ForwardTime = forwardTime;
                dbContext.RepeatInfo.Attach(existing).State = EntityState.Modified;
            }
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<EventRepeatInfo?> SetAutomaticDateGenerationForEventAsync(int eventId,
                                                                  DayOfWeek startFrom,
                                                                  DayOfWeekFlags dayOfWeekFlags,
                                                                  int numberOfWeeks)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existing = await dbContext.RepeatInfo
                .Include(ev => ev.Event)
                .FirstOrDefaultAsync(ev => ev.EventId == eventId);
            if (existing == null)
            {
                return null;
            }
            existing.NumberOfWeeksToGenerate = numberOfWeeks;
            existing.DaysOfWeekPattern = dayOfWeekFlags.ToPattern();
            existing.TargetDayOfWeek = startFrom;
            dbContext.RepeatInfo.Attach(existing).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task DisableAutomaticDateGenerationForEvent(int eventId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            await dbContext.RepeatInfo
                .Where(repeat => repeat.EventId == eventId)
                .ExecuteUpdateAsync(repeat => repeat
                    .SetProperty(r => r.DaysOfWeekPattern, _ => null)
                    .SetProperty(r => r.NumberOfWeeksToGenerate, _ => null)
                    .SetProperty(r => r.TargetDayOfWeek, _ => null));
            await dbContext.SaveChangesAsync();
        }
    }
}
