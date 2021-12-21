using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SignalrServer.Lib;
using SignalrServer.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SignalrServer.Controllers
{
    [Route("token-api")]
    [ApiController]
    public class TokenController : Controller
    {
        private readonly Config _config;

        public TokenController(Config config)
        {
            _config = config;
        }

        // [HttpOptions]

        [HttpGet("hubs")]
        [ProducesResponseType(typeof(TokenInfo), 200)]
        [ProducesResponseType(typeof(HttpErrorData), 400)]
        public IActionResult Hubs(
            [FromQuery] string userId,
            [FromQuery] int expiresMinutes)
        {

            var aud = "hubs";

            var claims = string.IsNullOrWhiteSpace(userId)
                ? Array.Empty<Claim>()
                : new[] { new Claim("sub", userId) };

            return GetOrCreateToken(expiresMinutes, aud, claims);
        }

        [HttpGet("rest-api")]
        [ProducesResponseType(typeof(TokenInfo), 200)]
        [ProducesResponseType(typeof(HttpErrorData), 400)]
        public IActionResult RestApi(
            [FromQuery] string grant,
            [FromQuery] int expiresMinutes)
        {
            if (!"all".Equals(grant, StringComparison.InvariantCulture))
            {
                return new BadRequestObjectResult(new HttpErrorData
                {
                    Error = "grant query string parameter must be grant=all"
                });
            }

            var aud = "rest-api";

            var claims = new[] { new Claim("grant", grant) };

            return GetOrCreateToken(expiresMinutes, aud, claims);
        }

        private IActionResult GetOrCreateToken(int expiresMinutes, string aud, Claim[] claims)
        {
            var secret = Auth.GetSecretFromAuthHeader(Request.Headers, out _);

            if (secret != _config.TokenGeneratorSharedSecret)
            {
                Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Realm\"");
                return new UnauthorizedResult();
            }
            var now = RoundDownToMinute(DateTimeOffset.Now);
            var expires = now.AddMinutes(expiresMinutes);


            var tokenString = CreateToken(aud, claims, now, expires);

            return Ok(new TokenInfo
            {
                Token = tokenString,
                Expiration = expires,
            });
        }

        private string CreateToken(string aud, Claim[] claims, DateTimeOffset now, DateTimeOffset expires)
        {
            var securityKey = new SymmetricSecurityKey(_config.TokenSymmetricKey);

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var header = new JwtHeader(credentials);


            var payload = new JwtPayload(null, aud, claims, now.UtcDateTime, expires.UtcDateTime);


            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();


            var tokenString = handler.WriteToken(secToken);
            return tokenString;
        }

        private static DateTimeOffset RoundDownToMinute(DateTimeOffset now)
        {
            return Floor(now, TimeSpan.FromMinutes(1));
        }
        public static DateTimeOffset Floor(DateTimeOffset dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }
    }
}
