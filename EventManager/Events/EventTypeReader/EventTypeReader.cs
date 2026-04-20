using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Events.EventReader
{
    public class EventTypeReader : SlashCommandTypeReader<ApplicationCommandContext>
    {
        public override ApplicationCommandOptionType Type => ApplicationCommandOptionType.String;
        public override Type? AutocompleteProviderType => typeof(EventAutoCompleteProvider);
        public override ValueTask<SlashCommandTypeReaderResult> ReadAsync(string value,
                                                                          ApplicationCommandContext context,
                                                                          SlashCommandParameter<ApplicationCommandContext> parameter,
                                                                          ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration,
                                                                          IServiceProvider? serviceProvider)
        {
            if (!int.TryParse(value, out var id))
            {
                return ValueTask.FromResult(SlashCommandTypeReaderResult.Fail(value));
            }
            return ValueTask.FromResult(SlashCommandTypeReaderResult.Success(id));
        }
    }
}
