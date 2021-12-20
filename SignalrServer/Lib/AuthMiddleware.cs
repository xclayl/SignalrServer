using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using SignalrServer.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SignalrServer.Lib
{
    public class AuthMiddleware
    {
        public static async Task Use(HttpContext context, Func<Task> next, Config config)
        {

            string token = null;
            var askForPassword = true;

            var path = context.Request.Path;
            if (path.StartsWithSegments("/token-api"))
            {
                await next();
                return;
            }

            if (context.Request.Method == "OPTIONS")
            {
                await next();
                return;
            }

            if (path.StartsWithSegments("/hubs"))
            {
                askForPassword = false;
                var accessToken = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    token = accessToken;
                }

            }

            token = token ?? Auth.GetSecretFromAuthHeader(context.Request.Headers, out askForPassword);


            if (!ValidToken(token, config.Token_Symmetric_Key_Base64, out var claimsPrincipal))
            {
                if (askForPassword)
                    context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Realm\"");
                context.Response.StatusCode = 401;
                return;
            }

            context.User = claimsPrincipal;

            await next();
        }

        private static bool ValidToken(string token, string key, out ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var mySecurityKey = new SymmetricSecurityKey(Convert.FromBase64String(key));

                var tokenHandler = new JwtSecurityTokenHandler();
                claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = mySecurityKey
                }, out var validatedToken);
            }
            catch
            {
                claimsPrincipal = null;
                return false;
            }
            return true;
        }
    }
}
