using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader
{
    internal class DayOfWeekTypeReader : SlashCommandTypeReader<ApplicationCommandContext>
    {
        public override ApplicationCommandOptionType Type => ApplicationCommandOptionType.String;
        public override Type? AutocompleteProviderType => typeof(DayOfWeekAutoCompleteProvider);
        public override int? GetMinLength(SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration)
        {
            return 7;
        }
        public override int? GetMaxLength(SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration)
        {
            return 7;
        }
        public override ValueTask<SlashCommandTypeReaderResult> ReadAsync(string value,
                                                                          ApplicationCommandContext context,
                                                                          SlashCommandParameter<ApplicationCommandContext> parameter,
                                                                          ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration,
                                                                          IServiceProvider? serviceProvider)
        {
            if (!DayOfWeekFlagsExtensions.TryParse(value, out DayOfWeekFlags result))
            {
                return ValueTask.FromResult(SlashCommandTypeReaderResult.Fail(value));
            }
            return ValueTask.FromResult(SlashCommandTypeReaderResult.Success(result));
        }
    }
}
