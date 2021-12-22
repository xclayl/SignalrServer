using Microsoft.AspNetCore.Http;
using System;
using System.Text;

namespace SignalRServer.Lib
{
    public static class Auth
    {
        public static string GetSecretFromAuthHeader(IHeaderDictionary requestHeaders, out bool askForPassword)
        {
            askForPassword = true;

            if (!requestHeaders.ContainsKey("Authorization"))
                return null;

            try
            {
                string headerVal = requestHeaders["Authorization"];

                if (headerVal.StartsWith("Basic "))
                {
                    var basicEncoded = headerVal.Substring(6);
                    var bytes = Convert.FromBase64String(basicEncoded);
                    var str = Encoding.ASCII.GetString(bytes);
                    var cPos = str.IndexOf(':');
                    if (cPos >= 0)
                    {
                        var pass = str.Substring(cPos + 1);
                        return pass;
                    }
                }
                else if (headerVal.StartsWith("Bearer "))
                {
                    askForPassword = false;
                    return headerVal.Substring(7);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return null;
        }
    }
}
