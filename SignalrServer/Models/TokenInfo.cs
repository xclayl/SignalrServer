using System;

namespace SignalrServer.Models
{
    public class TokenInfo
    {
        public string Token { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
