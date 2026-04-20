using EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader;
using Microsoft.Extensions.Logging;
using NetCord.Rest;

namespace EventManager.GenerateDates
{
    public class GenerateDatesService(RestClient client, ILogger<GenerateDatesService> logger)
    {
        public async Task<string?> GenerateDatesAsync(ulong channelId,
                                                      DateTime dateFrom,
                                                      DateTime dateUntil,
                                                      DayOfWeekFlags daysOfWeek)
        {
            logger.LogInformation("Generate dates executing for channel {channel}, {startdate} - {enddate} for days {days}", channelId, dateFrom.ToString("dd-MM-yyyy"), dateUntil.ToString("dd-MM-yyyy"), daysOfWeek);
            List<DateTime> dates = [];
            if (dateUntil >= dateFrom)
            {
                DateTime current = dateFrom;
                while (current <= dateUntil)
                {
                    if (daysOfWeek.Contains(current.DayOfWeek))
                    {
                        dates.Add(current);
                    }
                    current = current.AddDays(1);
                }
            }
            if (dateUntil < dateFrom || dates.Count > 30)
            {
                return string.Format("Invalid dates {0} - {1}", dateFrom.ToString("dd-MM-yyyy"), dateUntil.ToString("dd-MM-yyyy"));
            }
            logger.LogInformation("Generate dates with {number} unique dates", dates.Count);

            List<RestMessage> messages = [];

            foreach (DateTime current in dates)
            {
                var message = await client.SendMessageAsync(channelId, current.ToString("dddd dd MMMM yyyy"));
                messages.Add(message);
            }

            foreach (var message in messages)
            {
                await message.AddReactionAsync(new ReactionEmojiProperties("✅"));
                await message.AddReactionAsync(new ReactionEmojiProperties("〽️"));
                await message.AddReactionAsync(new ReactionEmojiProperties("❌"));
                await message.AddReactionAsync(new ReactionEmojiProperties("🟩"));
            }
            return null;
        }
    }
}
