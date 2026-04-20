using NetCord;
using NetCord.Services.ApplicationCommands;
using System.Globalization;

namespace EventManager.GenerateDates.TypeReaders.DateTypeReader
{
    internal class TimeOnlyTypeReader : SlashCommandTypeReader<ApplicationCommandContext>
    {
        public override ApplicationCommandOptionType Type => ApplicationCommandOptionType.String;
        public override Type? AutocompleteProviderType => typeof(TimeOnlyAutoCompleteProvider);
        public override int? GetMinLength(SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration)
        {
            return 5;
        }
        public override int? GetMaxLength(SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration)
        {
            return 5;
        }
        public override ValueTask<SlashCommandTypeReaderResult> ReadAsync(string value,
                                                                          ApplicationCommandContext context,
                                                                          SlashCommandParameter<ApplicationCommandContext> parameter,
                                                                          ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration,
                                                                          IServiceProvider? serviceProvider)
        {
            if (!TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.None, out TimeOnly result))
            {
                return ValueTask.FromResult(SlashCommandTypeReaderResult.Fail(value));
            }
            return ValueTask.FromResult(SlashCommandTypeReaderResult.Success(result));
        }
    }
}
