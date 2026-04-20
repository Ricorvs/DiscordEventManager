using NetCord;
using NetCord.Services.ApplicationCommands;
using System.Globalization;

namespace EventManager.GenerateDates.TypeReaders.DateTypeReader
{
    internal class DateTypeReader : SlashCommandTypeReader<ApplicationCommandContext>
    {
        public override ApplicationCommandOptionType Type => ApplicationCommandOptionType.String;
        public override Type? AutocompleteProviderType => typeof(DateAutoCompleteProvider);
        public override int? GetMinLength(SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration)
        {
            return 10;
        }
        public override int? GetMaxLength(SlashCommandParameter<ApplicationCommandContext> parameter, ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration)
        {
            return 10;
        }
        public override ValueTask<SlashCommandTypeReaderResult> ReadAsync(string value,
                                                                          ApplicationCommandContext context,
                                                                          SlashCommandParameter<ApplicationCommandContext> parameter,
                                                                          ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration,
                                                                          IServiceProvider? serviceProvider)
        {
            if (!DateTime.TryParseExact(value, "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
            {
                return ValueTask.FromResult(SlashCommandTypeReaderResult.Fail(value));
            }
            return ValueTask.FromResult(SlashCommandTypeReaderResult.Success(result));
        }
    }
}
