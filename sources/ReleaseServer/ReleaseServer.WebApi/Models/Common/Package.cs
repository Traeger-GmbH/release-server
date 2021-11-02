//--------------------------------------------------------------------------------------------------
// <copyright file="Package.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Package : IDisposable
    {
        #region ---------- Public const fields ----------

        /// <summary>
        /// 
        /// </summary>
        public const string InformationFileName = "package.json";

        #endregion

        #region ---------- Public constructor ----------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageFile"></param>
        public Package(IFormFile packageFile)
        {
            PackageInformation packageInformation;
            var validationErrors = new List<string>();

            using var fileStream = packageFile.OpenReadStream();
            this.ZipArchive = new ZipArchive(packageFile.OpenReadStream(), ZipArchiveMode.Read);

            var rootDirectoryName = "";
            ZipArchiveEntry packageInformationEntry = null;

            // Get root directory name and package.json entry
            for (var i = 0; i < this.ZipArchive.Entries.Count; i++) {
                var zipEntry = this.ZipArchive.Entries[i];
                var fileName = Path.GetFileName(zipEntry.FullName);

                if (fileName != null && fileName == InformationFileName) {
                    packageInformationEntry = zipEntry;
                    rootDirectoryName = Path.GetDirectoryName(zipEntry.FullName);
                }
            }

            if (packageInformationEntry != null) {
                try {
                    packageInformation = PackageInformation.FromJsonFile(packageInformationEntry);
                    this.Deployments = new Dictionary<Platform, ZipArchiveEntry>();

                    foreach (var (platformString, filePath) in packageInformation.Deployments) {
                        if (Platform.TryParse(platformString, out var platform)) {
                            var deploymentEntryName = rootDirectoryName + "/" + filePath;
                            if (this.ZipArchive.TryGetEntry(deploymentEntryName, out var deploymentEntry)) {
                                this.Deployments.Add(platform, deploymentEntry);
                            }
                            else {
                                var error = $"Missing deployment file for platform \"{platform}\": {filePath}";
                                validationErrors.Add(error);
                            }
                        }
                        else {
                            var error = $"Invalid platform in deployments: \"{platformString}\"";
                            validationErrors.Add(error);
                        }
                    }

                    if (validationErrors.Count == 0) {
                        this.ValidationResult = new ValidationResult(true);
                        this.Identifier = packageInformation.Identifier;
                        this.ReleaseNotes = packageInformation.ReleaseNotes;
                        this.Version = packageInformation.Version;
                    }
                    else {
                        this.ValidationResult = new ValidationResult(false, validationErrors);
                    }
                }
                catch (Exception e) {
                    var error = $"{Package.InformationFileName} is invalid: {e.Message}";
                    validationErrors.Add(error);
                    this.ValidationResult = new ValidationResult(false, validationErrors);
                }
            }
            else {
                var error = $"{Package.InformationFileName} is missing.";
                validationErrors.Add(error);
                this.ValidationResult = new ValidationResult(false, validationErrors);
            }
        }

        #endregion

        #region ---------- Public properties ----------

        ///// <summary>
        ///// 
        ///// </summary>
        //public IFormFile packageFile { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ZipArchive ZipArchive { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReleaseNotes ReleaseNotes { get; }

        /// <summary>
        /// 
        /// </summary>
        public ProductVersion Version { get; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<Platform, ZipArchiveEntry> Deployments { get; }

        /// <summary>
        /// 
        /// </summary>
        public ValidationResult ValidationResult { get; }


        #endregion

        #region ---------- Public methodss ----------

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (this.ZipArchive != null) {
                this.ZipArchive.Dispose();
            }
        }

        #endregion
    }
}