using EventManager.Events.Services;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EventManager.Events
{
    public class DiscordEventGatewayHandler(EventRegistrationService eventRegistrationService) : IGuildScheduledEventCreateGatewayHandler,
                                                                                                 IGuildScheduledEventDeleteGatewayHandler,
                                                                                                 IGuildScheduledEventUpdateGatewayHandler,
                                                                                                 IGuildScheduledEventUserAddGatewayHandler,
                                                                                                 IGuildScheduledEventUserRemoveGatewayHandler
    {

        async ValueTask IGuildScheduledEventCreateGatewayHandler.HandleAsync(GuildScheduledEvent arg)
        {
            await eventRegistrationService.HandleEventAdded(arg);
        }
        async ValueTask IGuildScheduledEventUpdateGatewayHandler.HandleAsync(GuildScheduledEvent arg)
        {
            if (arg.Status == GuildScheduledEventStatus.Completed || arg.Status == GuildScheduledEventStatus.Canceled)
            {
                await eventRegistrationService.HandleEventCompleted(arg);
            }
            else if (arg.Status == GuildScheduledEventStatus.Active)
            {
                await eventRegistrationService.HandleEventStarted(arg);
            }
            else
            {
                await eventRegistrationService.HandleEventChanged(arg);
            }
        }

        async ValueTask IGuildScheduledEventDeleteGatewayHandler.HandleAsync(GuildScheduledEvent arg)
        {
            await eventRegistrationService.HandleEventCompleted(arg);
        }

        async ValueTask IGuildScheduledEventUserAddGatewayHandler.HandleAsync(GuildScheduledEventUserEventArgs arg)
        {
            await eventRegistrationService.HandleEventUserInterested(arg.GuildScheduledEventId, arg.UserId);
        }

        async ValueTask IGuildScheduledEventUserRemoveGatewayHandler.HandleAsync(GuildScheduledEventUserEventArgs arg)
        {
            await eventRegistrationService.HandleEventUserNotInterested(arg.GuildScheduledEventId, arg.UserId);
        }
    }
}
