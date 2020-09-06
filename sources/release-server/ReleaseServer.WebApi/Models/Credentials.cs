//--------------------------------------------------------------------------------------------------
// <copyright file="Credentials.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the credentials of an user.
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The specified username.</value>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The specified password of the user.</value>
        public string Password { get; set; }
    }
}