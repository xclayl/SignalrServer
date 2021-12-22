using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRServer.Hubs;
using SignalRServer.Models;
using System;
using System.Threading.Tasks;

namespace SignalRServer.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [EnableCors(Startup.RestCors)]
    public class V1Controller : Controller
    {
        private readonly IHubContext<DefaultHub> _context;

        public V1Controller(IHubContext<DefaultHub> context)
        {
            _context = context;
        }




        [HttpPost("hubs/{hub}/users/{userId}")]
        public async Task AnnounceToUser([FromRoute] string hub, [FromRoute] string userId,
            PayloadMessage message)
        {
            if (hub != "default")
                throw new ArgumentException("The hub name must be 'default'. Found " + hub);

            await _context.Clients.User(userId).SendCoreAsync(message.Target, message.Arguments ?? Array.Empty<object>());
        }


        [HttpPost("hubs/{hub}/groups/{group}")]
        public async Task AnnounceToGroup([FromRoute] string hub, [FromRoute] string group, [FromQuery] string[] excluded, PayloadMessage message)
        {
            if (hub != "default")
                throw new ArgumentException("The hub name must be 'default'. Found " + hub);

            await _context.Clients.GroupExcept(group, excluded).SendCoreAsync(message.Target, message.Arguments ?? Array.Empty<object>());
        }

        [HttpPut("hubs/{hub}/groups/{group}/connections/{connectionId}")]
        public async Task JoinRoom([FromRoute] string hub, [FromRoute] string group, [FromRoute] string connectionId)
        {
            if (hub != "default")
                throw new ArgumentException("The hub name must be 'default'. Found " + hub);
            await _context.Groups.AddToGroupAsync(connectionId, group);
        }

        [HttpDelete("hubs/{hub}/groups/{group}/connections/{connectionId}")]
        public async Task LeaveRoom([FromRoute] string hub, [FromRoute] string group, [FromRoute] string connectionId)
        {
            if (hub != "default")
                throw new ArgumentException("The hub name must be 'default'. Found " + hub);
            await _context.Groups.RemoveFromGroupAsync(connectionId, group);
        }

    }
}
