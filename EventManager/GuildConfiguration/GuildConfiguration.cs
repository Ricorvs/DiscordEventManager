using Microsoft.EntityFrameworkCore;

namespace EventManager.GuildConfiguration
{
    [PrimaryKey(nameof(Id))]
    public class GuildConfiguration
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong? EventChannel { get; set; }
        public int? ThreadKeepAliveTime { get; set; }
        public TimeOnly? ThreadRefreshTime { get; set; }
    }
}
