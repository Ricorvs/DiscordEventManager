using EventManager.GenerateDates.TypeReaders.DateTypeReader;
using EventManager.GenerateDates.TypeReaders.DayOfWeekTypeReader;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EventManager.GenerateDates
{
    public class GenerateDatesModule(GenerateDatesService generateDatesService) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("generate-dates", "Generate a range of dates for availability")]
        public async Task GenerateDatesAsync(
            [SlashCommandParameter(Name = Constants.DateFrom, Description = "Start date in range in dd-MM-yyyy format", TypeReaderType = typeof(DateTypeReader))] DateTime dateFrom,
            [SlashCommandParameter(Name = Constants.DateUntil, Description = "End date in range in dd-MM-yyyy format", TypeReaderType = typeof(DateTypeReader))] DateTime dateUntil,
            [SlashCommandParameter(Name = "days", Description = "Days to generate", TypeReaderType = typeof(DayOfWeekTypeReader))] DayOfWeekFlags daysOfWeek)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            string? error = await generateDatesService.GenerateDatesAsync(Context.Interaction.Channel.Id, dateFrom, dateUntil, daysOfWeek);

            if (!string.IsNullOrEmpty(error))
            {
                await Context.Interaction.ModifyResponseAsync(message => message.WithContent(error));
                return;
            }

            await Context.Interaction.DeleteResponseAsync();
        }

    }
}
