using EventManager.Events.Models;
using EventManager.Events.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCord.Rest;
using System.Globalization;
using System.Text;

namespace EventManager.Calendar
{
    [Route("/")]
    [ApiController]
    public class CalendarService(RestClient client, EventService eventService, ILogger<CalendarService> logger) : ControllerBase()
    {
        private static string GetHeader(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, "BEGIN:VCALENDAR\r\nCALSCALE:GREGORIAN\r\nPRODID:EventManager\r\nVERSION:2.0\r\nMETHOD:PUBLISH\r\nX-PUBLISHED-TTL:PT1H\r\nX-WR-CALNAME:{0}\r\n", name);
        }

        private static string LimitLineLength(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 75)
            {
                return input;
            }
            string result = string.Empty;
            while (input.Length >= 70)
            {
                result += input.Substring(0, 70) + "\r\n ";
                input = input.Substring(70);
            }
            return result + input;
        }

        private static string? NormalizeLineEndings(string? input)
        {
            return input?.ReplaceLineEndings("\\n");
        }

        private const string _dateFormat = "yyyyMMddTHHmmssZ";

        private static string GetEventEntry(DiscordEvent ev)
        {
            return
            "BEGIN:VEVENT\r\n" +
            LimitLineLength(string.Format("UID:Eventmanager.tt/{0}\r\n", ev.EventId)) +
            LimitLineLength(string.Format("DTSTAMP:{0}\r\n", DateTime.UtcNow.ToString(_dateFormat))) +
            LimitLineLength(string.Format("DTSTART:{0}\r\n", ev.StartDateTime.ToString(_dateFormat, CultureInfo.CurrentCulture))) +
            LimitLineLength(string.Format("DTEND:{0}\r\n", (ev.EndDateTime ?? ev.StartDateTime.AddHours(1)).ToString(_dateFormat, CultureInfo.CurrentCulture))) +
            LimitLineLength(string.Format("SUMMARY:{0}\r\n", NormalizeLineEndings(ev.Name))) +
            LimitLineLength(string.Format("DESCRIPTION:{0}\r\n", NormalizeLineEndings(ev.Description))) +
            "STATUS:CONFIRMED\r\n" +
            "SEQUENCE:0\r\n" +
            "END:VEVENT\r\n";
        }

        [HttpGet("/")]
        public async Task<FileResult?> GetCalendarAsync([FromQuery] ulong guild, [FromQuery] ulong? user = null)
        {
            RestGuild? guildInstance = null;
            try
            {
                guildInstance = await client.GetGuildAsync(guild);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error obtaining calendar");
                return null;
            }
            string name;
            if (user != null)
            {
                var userInstance = await client.GetUserAsync(user.Value);
                name = $"{guildInstance.Name}/{userInstance.GlobalName}";
            }
            else
            {
                name = guildInstance.Name;
            }
            StringBuilder calendarBuilder = new();
            calendarBuilder.Append(GetHeader(name));
            var events = await eventService.GetEventsAsync(guild, user, true);

            foreach (var ev in events)
            {
                if (ev != null)
                {
                    calendarBuilder.Append(GetEventEntry(ev));
                }
            }
            calendarBuilder.Append("END:VCALENDAR");
            var result = new FileContentResult(Encoding.UTF8.GetBytes(calendarBuilder.ToString()), "text/calendar");
            return result;
        }
    }
}
