using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader
{
    public class DayOfWeekAutoCompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            List<ApplicationCommandOptionChoiceProperties> options = [];
            if (string.IsNullOrEmpty(option.Value))
            {
                options.Add(new("All      (0000000)", "0000000"));
                options.Add(new("Weekdays (00000__)", "00000__"));
                options.Add(new("Weekends (_____00)", "_____00"));
                options.Add(new("Long weekends (____000)", "____000"));
            }
            if (!string.IsNullOrEmpty(option.Value))
            {
                if (option.Value.Length != 7)
                {
                    string filledWithIgnore = option.Value.PadRight(7, '_');
                    string filledWithUse = option.Value.PadRight(7, '0');
                    options.Add(new(filledWithIgnore, filledWithIgnore));
                    options.Add(new(filledWithUse, filledWithUse));
                }
                else if (!options.Any(opt => opt.StringValue == option.Value))
                {
                    options.Add(new(option.Value, option.Value));
                }
            }
            return ValueTask.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(options);
        }
    }
}
