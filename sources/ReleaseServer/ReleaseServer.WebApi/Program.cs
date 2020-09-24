//--------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// This class provides the entry point for the ReleaseServer.WebApi application. It is used
    /// to setup the <see cref="IHostBuilder"/>.
    /// </summary>
    public class Program
    {
        #region ---------- Public static methods ----------
        
        /// <summary>
        /// The "main" method of the application.
        /// </summary>
        /// <param name="args">The arguments of the application.</param>
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Creates and configures the host.
        /// </summary>
        /// <param name="args">The arguments of the application.</param>
        /// <returns>The created host.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddDebug();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
            
        #endregion
    }
}