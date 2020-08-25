//--------------------------------------------------------------------------------------------------
// <copyright file="BasicAuthHandler.cs" company="Traeger IndustryComponents GmbH">
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
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            this.configuration = configuration;
        }
        
        #endregion
        
        #region ---------- Methods ----------
        
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
            //Extract base 64 encoded credentials from the header
            var authPayload = Convert.FromBase64String(authHeader.Parameter);
            
            var readableAuthPayload = Encoding.UTF8.GetString(authPayload);
            var credentials = readableAuthPayload.Split(":", StringSplitOptions.None);
            
            return new Credentials
            {
                Username = credentials[0],
                Password = credentials[1]
            };
        }

        private async Task<string> CheckCredentials(Credentials credentials)
        {
            //Didn't have to be async, but later if there are DB operations needed.
            var validCredentials = await Task.Run(() =>
                new Credentials{
                    Username = configuration["Credentials:Username"],
                    Password = configuration["Credentials:Password"]});
            
            if (CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(credentials.Username), Encoding.UTF8.GetBytes(validCredentials.Username)) && 
                CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(credentials.Password), Encoding.UTF8.GetBytes(validCredentials.Password)))
                return validCredentials.Username;
            
            return null;
        }
        
        #endregion
        #endregion
    }
}