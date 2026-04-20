using EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Globalization;

namespace EventManager.GenerateDates.TypeReaders.DateTypeReader
{
    public class DateAutoCompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        private static ApplicationCommandOptionChoiceProperties DateTimeToChoice(DateTime dateTime) =>
            new(dateTime.ToString("dddd dd MMMM yyyy"), dateTime.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture));
        private const int NumberOfSuggestions = 4;
        internal virtual DayOfWeek GetTargetDayOfWeek(ApplicationCommandInteractionDataOption option)
        {
            if (option.Name == Constants.DateFrom)
            {
                return DayOfWeek.Monday;
            }
            else
            {
                return DayOfWeek.Sunday;
            }
        }
        internal DateTime GetReferenceDate(ApplicationCommandInteractionDataOption option,
                                           AutocompleteInteractionContext context,
                                           out bool isEndDate)
        {
            if (option.Name == Constants.DateUntil &&
                context.Interaction.Data.Options.FirstOrDefault(option => option.Name == Constants.DateFrom) is ApplicationCommandInteractionDataOption firstDate &&
                DateTime.TryParseExact(firstDate.Value, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime target))
            {
                isEndDate = true;
                return target;
            }
            isEndDate = false;
            return DateTime.Today;
        }
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            DateTime startingReferenceDate = GetReferenceDate(option, context, out bool isEndDate);
            DayOfWeek targetDayOfWeek = GetTargetDayOfWeek(option);
            List<DateTime>? options = null;
            if (!string.IsNullOrEmpty(option.Value) && char.IsLetter(option.Value[0]))
            {
                options = GetChoicesForText(option.Value, startingReferenceDate);
            }
            else if (option.Value != null && option.Value.Length <= 2 && int.TryParse(option.Value, out int day) && day <= 31)
            {
                options = GetChoicesForDatePrefill(option.Value, startingReferenceDate, day);
            }
            options ??= GetDefaultChoices(startingReferenceDate, targetDayOfWeek, isEndDate);
            options.Sort();
            return ValueTask.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(options.Select(DateTimeToChoice));
        }

        private static List<DateTime> GetDefaultChoices(DateTime startingReferenceDate, DayOfWeek targetDayOfWeek, bool isEndDate)
        {
            List<DateTime> options = [];
            if (!isEndDate)
            {
                options.Add(startingReferenceDate);
            }
            DateTime nextTargetDay = startingReferenceDate.AddDays((7 + (int)targetDayOfWeek - (int)startingReferenceDate.DayOfWeek) % 7);
            for (int i = 0; i < NumberOfSuggestions; i++)
            {
                options.Add(nextTargetDay.AddDays(7 * i));
            }
            return options;
        }

        private static List<DateTime>? GetChoicesForText(string text, DateTime startingReferenceDate)
        {
            DayOfWeekFlags flags = DayOfWeekFlagsExtensions.GetFromSearchText(text);
            if (flags == 0)
            {
                return null;
            }
            List<DateTime> options = [];
            DateTime current = startingReferenceDate;
            for (int i = 0; i < NumberOfSuggestions * 7; i++)
            {
                if (flags.Contains(current.DayOfWeek))
                {
                    options.Add(current);
                }
                current = current.AddDays(1);
            }
            return options;
        }

        private static List<DateTime> GetChoicesForDatePrefill(string prefill, DateTime startingReferenceDate, int day)
        {
            List<DateTime> options = [];
            DateTime referenceDate;
            while (!DateTime.TryParseExact($"{day}-{startingReferenceDate:MM-yyyy}", "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out referenceDate))
            {
                startingReferenceDate = startingReferenceDate.AddMonths(1);
            }
            if (day <= 3 && DateTime.TryParseExact($"{day}0-{referenceDate:MM-yyyy}", "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dayPrefixReferenceDate))
            {
                while (dayPrefixReferenceDate.Day.ToString().StartsWith(prefill))
                {
                    DateTime target = dayPrefixReferenceDate > startingReferenceDate
                                    ? dayPrefixReferenceDate
                                    : dayPrefixReferenceDate.AddMonths(1);
                    if (target.Day.ToString().StartsWith(prefill))
                    {
                        options.Add(target);
                    }
                    dayPrefixReferenceDate = dayPrefixReferenceDate.AddDays(1);
                }
            }
            if (referenceDate < startingReferenceDate)
            {
                referenceDate = referenceDate.AddMonths(1);
            }
            for (int i = 0; i < NumberOfSuggestions; i++)
            {
                options.Add(referenceDate.AddMonths(i));
            }
            return options;
        }
    }
}
