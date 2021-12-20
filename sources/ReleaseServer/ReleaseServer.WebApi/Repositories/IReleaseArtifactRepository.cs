//--------------------------------------------------------------------------------------------------
// <copyright file="IReleaseArtifactRepository.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO.Compression;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// An interface that provides methods for interacting with the repository, where the <see cref="ReleaseArtifact"/> objects get stored.
    /// The methods provide the possibility to store / delete / get <see cref="ReleaseArtifact"/> objects and query specific artifact information.
    /// </summary>
    public interface IReleaseArtifactRepository     
    {
        /// <summary>
        /// Stores the <see cref="ReleaseArtifact"/> in the repository.
        /// </summary>
        /// <param name="artifact">The release artifact that will be stored.</param>
        void StoreArtifact(ReleaseArtifact artifact);

        /// <summary>
        /// Gets a list of all stored products.
        /// </summary>
        /// <returns></returns>
        List<string> GetProductList();

        /// <summary>
        /// Gets the disk usage of the repository.
        /// </summary>
        /// <returns></returns>
        DiskUsage GetDiskUsage();

        /// <summary>
        /// Gets the number of stored products.
        /// </summary>
        /// <returns></returns>
        int GetNumberOfProducts();

        /// <summary>
        /// Gets the number of stored artifacts.
        /// </summary>
        /// <returns></returns>
        int GetNumberOfArtifacts();

        /// <summary>
        /// Retrieves the product information of specific product name.
        /// </summary>
        /// <param name="productName">The name of the specific product.</param>
        /// <returns>The corresponding <see cref="DeploymentInformation"/> in form of a <see cref="List{T}"/>.</returns>
        List<DeploymentInformation> GetInfosByProductName(string productName);

        /// <summary>
        /// Retrieves the release information of a specific artifact.
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <returns>The corresponding <see cref="DeploymentInformation"/>.</returns>
        DeploymentInformation GetDeploymentInformation(string productName, string os, string architecture, string version);
        
        /// <summary>
        /// Retrieves a specific release artifact and converts it into a <see cref="ArtifactDownload"/> object.
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <returns>The created <see cref="ArtifactDownload"/> object.</returns>
        ArtifactDownload GetSpecificArtifact(string productName, string os, string architecture, string version);
        
        /// <summary>
        /// Deletes a specific release artifact, if it exists.
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <returns>The <see cref="bool"/> variable that describes the state of the operation ("true" if the product existed and the
        /// deletion was successful, "false" if the product did not exist)</returns>
        bool DeleteSpecificArtifactIfExists(string productName, string os, string architecture, string version);
        
        /// <summary>
        /// Deletes all artifacts of a specific product name, if it exists.
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <returns>The <see cref="bool"/> variable that describes the state of the operation ("true" if the artifacts existed and the
        /// deletion was successful, "false" if the products did not exist)</returns>
        bool DeleteProductIfExists(string productName);
        
        /// <summary>
        /// Retrieves a list of all available versions of the specified artifact.
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ProductVersion"/> objects, that fit the given arguments.</returns>
        List<ProductVersion> GetVersions(string productName, string os, string architecture);
        
        /// <summary>
        /// Retrieves all available platforms for a specific artifact.
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <returns>A <see cref="List{T}"/> <see cref="string"/> objects with the corresponding versions.</returns>
        List<string> GetPlatforms(string productName, string version);
        
        /// <summary>
        /// Creates a backup of the whole repository.
        /// </summary>
        /// <returns>The created <see cref="BackupInformation"/>.</returns>
        BackupInformation RunBackup();
        
        /// <summary>
        /// Restores the repository, based on the given "backupPayload".
        /// Note: The restore operation deletes the former content of the repository!
        /// </summary>
        /// <param name="backupPayload">The backup payload in form of a <see cref="ZipArchive"/></param>
        void RestoreBackup(ZipArchive backupPayload);
    }
}