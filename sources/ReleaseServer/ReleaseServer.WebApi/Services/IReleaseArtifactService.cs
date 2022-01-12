//--------------------------------------------------------------------------------------------------
// <copyright file="IReleaseArtifactService.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// An interface for a service, that contains the business logic of the release-server (e.g. validation, conversion etc.).
    /// The implementation of this interface has to interact with an implementation of the <see cref="IReleaseArtifactRepository"/>.
    /// </summary>
    public interface IReleaseArtifactService
    {
        /// <summary>
        /// Gets a list of all uploaded products.
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetProductList();

        /// <summary>
        /// Gets the runtime statistics of the server.
        /// </summary>
        /// <returns></returns>
        Task<Statistics> GetStatistics();

        /// <summary>
        /// Stores the provided payload asynchronously based on the given arguments (productName, os, architecture and version).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <param name="payload">The provided payload with the release artifact in form of an <see cref="IFormFile"/>.</param>
        Task StoreArtifact(string productName, string os, string architecture, string version, IFormFile payload);

        /// <summary>
        /// Stores the provided payload asynchronously as package.
        /// </summary>
        /// <param name="payload">The provided payload with the package in form of an <see cref="IFormFile"/>.</param>
        /// <param name="force">Specifies if a package shall be overwritten when it already exists.</param>
        Task<ServiceActionResult> StorePackage(IFormFile payload, bool force = false);

        /// <summary>
        /// Retrieves the product information of specific product name (asynchronously).
        /// </summary>
        /// <param name="productName">The name of the specific product.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="DeploymentInformation"/> objects.</returns>
        Task<List<DeploymentInformation>> GetDeploymentInformations(string productName);
        
        /// <summary>
        /// Retrieves all available platforms for a specific artifact (asynchronously).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ProductVersion"/> objects, that fit the given arguments.</returns>
        Task<List<string>> GetPlatforms(string productName, string version);
        
        /// <summary>
        /// Retrieves the deployment information of a specific artifact (asynchronously).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <returns>The corresponding <see cref="DeploymentInformation"/>.</returns>
        Task<DeploymentInformation> GetDeploymentInformation(string productName, string os, string architecture, string version);
        
        /// <summary>
        /// Retrieves a list of all available versions of the specified artifact (asynchronously).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ProductVersion"/> objects, that fit the given arguments.</returns>
        Task<List<ProductVersion>> GetVersions(string productName, string os, string architecture);
        
        /// <summary>
        /// Retrieves the latest version of a the specified artifact (asynchronously).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <returns>The latest <see cref="ProductVersion"/>.</returns>
        Task<ProductVersion> GetLatestVersion(string productName, string os, string architecture);
        
        /// <summary>
        /// Retrieves a specific release artifact and converts it into a <see cref="ArtifactDownload"/> object (asynchronously).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <returns>The created <see cref="ArtifactDownload"/> object.</returns>
        Task<ArtifactDownload> GetSpecificArtifact(string productName, string os, string architecture, string version);
        
        /// <summary>
        /// Retrieves the latest release artifact with the specified criteria and converts it into a <see cref="ArtifactDownload"/> object
        /// (asynchronously).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <returns>The created <see cref="ArtifactDownload"/> object.</returns>
        Task<ArtifactDownload> GetLatestArtifact(string productName, string os, string architecture);
        
        /// <summary>
        /// Deletes a specific release artifact, if it exists (asynchronously).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <returns>The <see cref="bool"/> variable that describes the state of the operation ("true" if the product existed and the
        /// deletion was successful, "false" if the product did not exist)</returns>
        Task<bool> DeleteSpecificArtifactIfExists(string productName, string os, string architecture, string version);
        
        /// <summary>
        /// Deletes all artifacts of a specific product name, if it exists (asynchronously).
        /// </summary>
        /// <param name="productName">The product name of the release artifact.</param>
        /// <returns>The <see cref="bool"/> variable that describes the state of the operation ("true" if the artifacts existed and the
        /// deletion was successful, "false" if the products did not exist)</returns>
        Task<bool> DeleteProductIfExists(string productName);
        
        /// <summary>
        /// Creates a backup of all release artifacts asynchronously.
        /// </summary>
        /// <returns>The created <see cref="BackupInformation"/>.</returns>
        Task<BackupInformation> RunBackup();
        
        /// <summary>
        /// Restores the repository, based on the given "payload" asynchronously.
        /// Note: The restore operation deletes the former content of the repository!
        /// </summary>
        /// <param name="payload">The backup payload in form of an <see cref="IFormFile"/></param>
        Task RestoreBackup(IFormFile payload);
        
        /// <summary>
        /// Validates the uploaded payload, if it fulfills the specified criteria.
        /// </summary>
        /// <param name="payload">The uploaded payload.</param>
        /// <returns>The corresponding <see cref="ValidationResult"/>.</returns>
        ValidationResult ValidateUploadPayload(IFormFile payload);
    }
}
