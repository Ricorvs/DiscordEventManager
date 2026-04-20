using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EventManager.RefreshThreads
{
    public class RefreshThreadsModule(RefreshThreadsService refreshThreadsService) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("refresh-threads", "Refresh event threads")]
        public async Task RefreshThreadsasync()
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            await refreshThreadsService.RestoreAllThreadsAsync(Context.Interaction.Guild!);

            await Context.Interaction.DeleteResponseAsync();
        }
    }
}
