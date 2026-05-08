using EventManager.Events.Models;
using EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManager.EventRepeat
{
    [PrimaryKey(nameof(EventId))]
    public class EventRepeatInfo
    {
        public int? EventId { get; set; }
        [ForeignKey(nameof(EventId))]
        public DiscordEvent? Event { get; set; }
        public bool Repeat { get; set; }
        public int ForwardTime { get; set; }
        public DayOfWeek? TargetDayOfWeek { get; set; }
        public string? DaysOfWeekPattern { get; set; }
        public int? NumberOfWeeksToGenerate { get; set; }
        public int? DelayDateGeneration { get; set; }
        public bool HasAutomaticDateGeneration => TargetDayOfWeek != null && DaysOfWeekPattern != null && NumberOfWeeksToGenerate != null;
        public override string ToString()
        {
            if (!Repeat)
            {
                return $"**{Event?.Name}**\r\nRepeat disabled";
            }
            string message = $"**{Event?.Name}**\r\n{ForwardTime} days forward on repeat";
            if (HasAutomaticDateGeneration && DayOfWeekFlagsExtensions.TryParse(DaysOfWeekPattern, out DayOfWeekFlags result))
            {
                message += $"\r\nDate generation: First {TargetDayOfWeek} after target date.\r\nEvery {result} for {NumberOfWeeksToGenerate} weeks";
                if (DelayDateGeneration != null)
                {
                    message += $"\r\nWill execute {DelayDateGeneration} days after end of previous event";
                }
            }

            return message;
        }
    }
}
