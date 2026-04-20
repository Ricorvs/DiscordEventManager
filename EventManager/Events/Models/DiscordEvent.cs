using EventManager.EventRepeat;
using Microsoft.EntityFrameworkCore;
using NetCord;

namespace EventManager.Events.Models
{
    [PrimaryKey(nameof(Id))]
    public class DiscordEvent
    {
        public DiscordEvent() { }
        public DiscordEvent(GuildScheduledEvent scheduledEvent) : this()
        {
            GuildId = scheduledEvent.GuildId;
            Update(scheduledEvent);
        }
        public void Update(GuildScheduledEvent scheduledEvent)
        {
            EventId = scheduledEvent.Id;
            Name = scheduledEvent.Name;
            Description = scheduledEvent.Description;
            Location = scheduledEvent.Location;
            StartDateTime = scheduledEvent.ScheduledStartTime.UtcDateTime;
            EndDateTime = scheduledEvent.ScheduledEndTime?.UtcDateTime;
            EntityType = scheduledEvent.EntityType;
            EventChannelId = scheduledEvent.ChannelId;
        }
        public int? Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong EventId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong ThreadChannelId { get; set; }
        public string? InviteUrl { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool Expired { get; set; }
        public bool AutomaticallyCreated { get; set; }
        public GuildScheduledEventEntityType EntityType { get; set; }
        public ulong? EventChannelId { get; set; }
        public EventRepeatInfo? RepeatInfo { get; set; }
        public IEnumerable<EventUserInterest>? InterestedUsers { get; }
    }
}
