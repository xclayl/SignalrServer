using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.Hubs
{
    [EnableCors(Startup.HubsCors)]
    public class DefaultHub : Hub
    {

    }
}
