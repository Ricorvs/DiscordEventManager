using System.ComponentModel.DataAnnotations.Schema;

namespace EventManager.Events.Models
{
    public class EventUserInterest
    {
        public int? Id { get; set; }
        public int? EventId { get; set; }
        [ForeignKey(nameof(EventId))]
        public DiscordEvent? Event { get; set; }
        public ulong UserId { get; set; }
    }
}
