using EventManager.Events.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Events.EventReader
{
    public class EventAutoCompleteProvider(EventService eventService) : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            if (context.Interaction.GuildId == null)
            {
                return [];
            }
            var events = await eventService.GetEventsAsync(context.Interaction.GuildId.Value);
            if (!string.IsNullOrEmpty(option.Value))
            {
                events = events.Where(ev => ev!.Name!.Contains(option.Value));
            }
            return events
                .Select(ev => new ApplicationCommandOptionChoiceProperties(ev!.Name!, ev!.Id!.ToString()!));
        }
    }
}
