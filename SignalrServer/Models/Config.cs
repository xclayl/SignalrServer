using System.Collections.Generic;

namespace SignalRServer.Models
{
    public class Config
    {
        public string TokenGeneratorSharedSecret { get; init; }
        public byte[] TokenSymmetricKey { get; init; }
        public IReadOnlyList<string> RestApiCorsOrigins { get; init; }
        public IReadOnlyList<string> HubsCorsOrigins { get; init; }
        public bool AllowAnonymous { get; init; }
    }
}
