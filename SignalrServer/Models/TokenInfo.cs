using System;

namespace SignalRServer.Models
{
    public class TokenInfo
    {
        public string Token { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
