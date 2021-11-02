//--------------------------------------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Timo Walter</author>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidationResult
    {
        #region ---------- Public constructors ----------

        /// <summary>
        /// 
        /// </summary>
        public ValidationResult(bool isValid)
        {
            this.IsValid = isValid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="validationErrors"></param>
        public ValidationResult(bool isValid, IEnumerable<string> validationErrors)
            : this(isValid)
        {
            this.ValidationErrors = validationErrors;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="validationError"></param>
        public ValidationResult(bool isValid, string validationError)
            : this(isValid, new string[] { validationError } )
        { }

        #endregion

        #region ---------- Public properties ----------

        /// <summary>
        /// Gets or set the validation flag.
        /// </summary>
        /// <value>The validation flag is `true`, if it's valid and `false` if not.</value>
        public bool IsValid { get; }

        /// <summary>
        /// Gets or sets the validation errors.
        /// </summary>
        public IEnumerable<string> ValidationErrors { get; }

        #endregion
    }
}
