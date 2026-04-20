using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Globalization;

namespace EventManager.GenerateDates.TypeReaders.DateTypeReader
{
    public class TimeOnlyAutoCompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        private static TimeOnly[] Options = [.. Enumerable.Range(0, 24 * 4).Select(i => new TimeOnly(i / 4, i % 4 * 15))];

        private static ApplicationCommandOptionChoiceProperties TimeOnlyToChoice(TimeOnly time) =>
            new(time.ToString("HH:mm"), time.ToString("HH:mm", CultureInfo.CurrentCulture));
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            var options = Options.Select(TimeOnlyToChoice);
            if (!string.IsNullOrEmpty(option.Value))
            {
                options = options.Where(time => time.StringValue!.StartsWith(option.Value));
            }
            options = options.Take(25);
            return ValueTask.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>([.. options]);
        }
    }
}
