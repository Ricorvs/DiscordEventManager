using Microsoft.Extensions.Configuration;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Calendar
{
    public class GetCalendarMenu(IConfiguration configuration) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("get-personal-calendar", "Get calendar link for personal calendar")]
        public string GetPersonalCalendar()
        {
            string? link = configuration["calendarlink"];
            if (string.IsNullOrEmpty(link))
            {
                return "No link configured";
            }
            return $"{link}?guild={Context.Interaction.GuildId}&user={Context.Interaction.User.Id}";
        }
        [SlashCommand("get-server-calendar", "Get calendar link for server calendar")]
        public string GetServerCalendar()
        {
            string? link = configuration["calendarlink"];
            if (string.IsNullOrEmpty(link))
            {
                return "No link configured";
            }
            return $"{link}?guild={Context.Interaction.GuildId}";
        }

    }
}
