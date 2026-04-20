using EventManager.EventRepeat;
using EventManager.Events.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DbContext
{
    public class EventManagerDbContext(DbContextOptions<EventManagerDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
    {
        public DbSet<DiscordEvent> Events { get; set; } = default!;
        public DbSet<EventRepeatInfo> RepeatInfo { get; set; } = default!;
        public DbSet<EventUserInterest> UserEventInterest { get; set; } = default!;
        public DbSet<GuildConfiguration.GuildConfiguration> GuildConfiguration { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiscordEvent>()
                .HasMany(ev => ev.InterestedUsers)
                .WithOne(ev => ev.Event)
                .HasForeignKey(ev => ev.EventId)
                .IsRequired(false);
            modelBuilder.Entity<DiscordEvent>()
                .HasOne(ev => ev.RepeatInfo)
                .WithOne(ev => ev.Event)
                .HasForeignKey<EventRepeatInfo>(repeat => repeat.EventId)
                .IsRequired(false);
            base.OnModelCreating(modelBuilder);
        }
    }
}
