using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace SignalrServer.Hubs
{
    [EnableCors(Startup.HubsCors)]
    public class DefaultHub : Hub
    {

    }
}
