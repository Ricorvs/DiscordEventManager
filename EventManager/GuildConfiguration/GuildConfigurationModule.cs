using EventManager.GenerateDates.TypeReaders.DateTypeReader;
using EventManager.RefreshThreads;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Text;

namespace EventManager.GuildConfiguration
{
    public class GuildConfigurationModule(GuildConfigurationService guildConfigurationService,
                                          RefreshThreadsBackgroundService refreshThreadsBackgroundService) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("get-guild-config", "Get the current guild configuration")]
        public async Task GetGuildConfigAsync()
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            var config = await guildConfigurationService.GetGuildConfigurationAsync(Context.Interaction.GuildId!.Value);

            if (config == null)
            {
                await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent("Nothing configured for current guild"));
                return;
            }
            StringBuilder builder = new();
            builder.AppendLine($"Eventchannel: <#{config.EventChannel}>");
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent(builder.ToString()));
        }


        [SlashCommand("set-guild-eventchannel", "Set the current eventchannel for guild")]
        public async Task SetGuildEventChannelAsync([SlashCommandParameter(Description = "Channel to use for event threads")] TextGuildChannel channel)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            await guildConfigurationService.SetGuildEventChannelAsync(Context.Interaction.GuildId!.Value, channel.Id);
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent($"Set eventchannel to {channel}"));
        }
        [SlashCommand("set-guild-threadrefreshtime", "Set the thread refresh time for guild")]
        public async Task SetGuildEventChannelAsync([SlashCommandParameter(Description = "Time of day thread refresh should happen", TypeReaderType = typeof(TimeOnlyTypeReader))] TimeOnly refreshTime)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            var currentConfig = await guildConfigurationService.SetGuildThreadRefreshTimeAsync(Context.Interaction.GuildId!.Value, refreshTime);
            refreshThreadsBackgroundService.UpdateTimerTrigger(Context.Interaction.GuildId.Value, currentConfig);
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent($"Set refresh time to {refreshTime:HH:mm}"));
        }
        [SlashCommand("set-guild-threadkeepalivetime", "Set the current keep alive time for guild threads")]
        public async Task SetGuildThreadKeepAliveTimeAsync([SlashCommandParameter(Description = "How many days event threads should be kept alive after the event has finished", MinValue = 0)] int keepAliveTime)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            await guildConfigurationService.SetGuildThreadKeepAliveTimeAsync(Context.Interaction.GuildId!.Value, keepAliveTime);
            await Context.Interaction.ModifyResponseAsync(msg => msg.WithContent($"Set refresh time to {keepAliveTime}"));
        }
    }
}
