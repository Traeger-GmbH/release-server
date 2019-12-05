using System;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Auth
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly string AuthPath;
        
        public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            IConfiguration configuration,
            UrlEncoder encoder, 
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            AuthPath = configuration["AuthDirectory"];
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Authorization Header is missing");

            CredentialsModel sentCredentials; 

            try
            {
                //Extract base 64 encoded credentials from the header
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                sentCredentials = ExtractCredentialsFromHeader(authHeader);
            }
            catch 
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            var authenticatedUser = await CheckCredentials(sentCredentials);
           
            if (authenticatedUser != null)
            {
                var claims = new[] {
                    new Claim(ClaimTypes.Name, sentCredentials.Username)
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                
                return AuthenticateResult.Success(ticket);
            }
            
            return AuthenticateResult.Fail("Invalid credentials!");
        }

        private static CredentialsModel ExtractCredentialsFromHeader(AuthenticationHeaderValue authHeader)
        {
            //Extract base 64 encoded credentials from the header
            var authPayload = Convert.FromBase64String(authHeader.Parameter);
            
            var readableAuthPayload = Encoding.UTF8.GetString(authPayload);
            var credentials = readableAuthPayload.Split(":", StringSplitOptions.None);
            
            return new CredentialsModel
            {
                Username = credentials[0],
                Password = credentials[1]
            };
        }

        private async Task<string> CheckCredentials(CredentialsModel credentials)
        {
            //Get the credentials from the auth path and base64 decode the password
            var validCredentials = await Task.Run(() =>
                JsonConvert.DeserializeObject<CredentialsModel>(File.ReadAllText(AuthPath)));
            validCredentials.Password = Encoding.Default.GetString(Convert.FromBase64String(validCredentials.Password));

            if (credentials.Username == validCredentials.Username && credentials.Password == validCredentials.Password)
                return validCredentials.Username;
            
            return null;
        }
    }
}