//--------------------------------------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set;}

        public string ValidationError { get; set;}
    }
}