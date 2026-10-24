//--------------------------------------------------------------------------------------------------
// <copyright file="BasicAuthHandler.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ReleaseServer.WebApi.Models;


namespace ReleaseServer.WebApi
{
    internal class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        
        #region ---------- Private fields ----------
        
        private readonly IConfiguration configuration;
        
        #endregion
        
        #region ---------- Public constructors ----------

        public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            IConfiguration configuration,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
            this.configuration = configuration;
        }
        
        #endregion
        
        #region ---------- Protected methods ----------

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Authorization Header is missing");

            Credentials sentCredentials; 

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

            if (sentCredentials == null)
                return AuthenticateResult.Fail("Invalid Authorization Header");

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
        
        #endregion
        
        #region ---------- Private methods ----------
        
        private static Credentials ExtractCredentialsFromHeader(AuthenticationHeaderValue authHeader)
        {
            if (!string.Equals(authHeader.Scheme, "Basic", StringComparison.OrdinalIgnoreCase) || authHeader.Parameter == null)
                return null;

            //Extract base 64 encoded credentials from the header
            var authPayload = Convert.FromBase64String(authHeader.Parameter);
            
            var readableAuthPayload = Encoding.UTF8.GetString(authPayload);

            // The user name must not contain a colon (RFC 7617), the password may contain any character.
            var separatorIndex = readableAuthPayload.IndexOf(':');
            if (separatorIndex < 0)
                return null;

            return new Credentials
            {
                Username = readableAuthPayload.Substring(0, separatorIndex),
                Password = readableAuthPayload.Substring(separatorIndex + 1)
            };
        }

        private async Task<string> CheckCredentials(Credentials credentials)
        {
            //Didn't have to be async, but later if there are DB operations needed.
            var validCredentials = await Task.Run(() =>
                new Credentials{
                    Username = configuration["CREDENTIALS_USERNAME"],
                    Password = configuration["CREDENTIALS_PASSWORD"]});

            // Without configured credentials, nobody is authenticated.
            if (string.IsNullOrEmpty(validCredentials.Username) || string.IsNullOrEmpty(validCredentials.Password))
            {
                Logger.LogError("No credentials configured (CREDENTIALS_USERNAME / CREDENTIALS_PASSWORD). All write access is denied.");
                return null;
            }
            
            // Compare both values completely (no short circuit), so the response time does not reveal which one was wrong.
            var usernameMatches = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(credentials.Username), Encoding.UTF8.GetBytes(validCredentials.Username));
            var passwordMatches = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(credentials.Password), Encoding.UTF8.GetBytes(validCredentials.Password));

            if (usernameMatches && passwordMatches)
                return validCredentials.Username;
            
            return null;
        }
        
        #endregion
    }
}