//--------------------------------------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the information, if an artifact is valid or not.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or set the validation flag (true -> valid, false -> invalid).
        /// </summary>
        public bool IsValid { get; set;}

        /// <summary>
        /// Gets or sets the concrete validation error.
        /// </summary>
        public string ValidationError { get; set;}
    }
}