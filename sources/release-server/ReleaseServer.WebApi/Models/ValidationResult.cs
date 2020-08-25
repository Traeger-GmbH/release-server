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
        /// Gets or set the validation flag.
        /// </summary>
        /// <value>The validation flag is `true`, if it's valid and `false` if not.</value>
        public bool IsValid { get; set;}

        /// <summary>
        /// Gets or sets the concrete validation error.
        /// </summary>
        /// <value>The concrete validation error. The <see cref="string"/> is empty, if there exists no error.</value>
        public string ValidationError { get; set;}
    }
}