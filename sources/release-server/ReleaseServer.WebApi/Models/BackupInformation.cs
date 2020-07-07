//--------------------------------------------------------------------------------------------------
// <copyright file="BackupInformation.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    public class BackupInformation
    {
        public string FullPath { get; set; }
        
        public string FileName { get; set; }
    }
}