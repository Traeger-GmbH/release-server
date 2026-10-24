//--------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;

using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// This class provides the opportunity to configure the services and request processing pipeline of
    /// the ReleaseServer.WebApi application.
    /// </summary>
    public class Startup
    {
        #region ---------- Public properties ----------
        
        /// <summary>
        /// Gets the configuration of the application.
        /// </summary>
        /// <value>The specified configuration of the application.</value>
        public IConfiguration Configuration { get; }
        
        #endregion
        
        #region ---------- Public constructors ----------
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the host (appsettings.json of the content root,
        /// environment variables and command line arguments).</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        #endregion
        
        #region ---------- Public methods ----------
        
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> interface to add several service features to the application.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    JsonConfiguration.Configure(options);
                })
                .ConfigureApiBehaviorOptions(LegacyProblemDetails.Configure);

            services.AddSwaggerGen(c =>
            {
                
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Release Server API",
                    Version = "v1",
                    Description = "Download server for released software. " +
                                  "Products are published as releases (version, release date, change log) for one or more platforms " +
                                  "(operating system and architecture).\n\n" +
                                  "**Public endpoints** (no authentication):\n" +
                                  "- `GET /artifacts` lists all products\n" +
                                  "- `GET /artifacts/{product}/info` lists the releases of a product (filter, sort and paging)\n" +
                                  "- `GET /artifacts/{product}/{os}/{architecture}/{version}` downloads a release\n" +
                                  "- `GET /artifacts/{product}/{os}/{architecture}/latest` downloads the latest release\n\n" +
                                  "All other endpoints maintain the server and **require authentication** (HTTP Basic).\n\n" +
                                  "Errors are returned as problem details (RFC 7807) with the fields title, status and detail.",
                    Contact = new OpenApiContact
                    {
                        Name = "Traeger Industry Components GmbH",
                        Email = "info@traeger.de",
                        Url = new Uri("https://www.traeger.de")
                    }
                });
                
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Name = "Authorization",
                    Scheme = "basic", 
                    Description = "Input your username and password to access this API",
                    In = ParameterLocation.Header,
                });
                
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ReleaseServer.WebApi.xml"));

                // Registered after the XML comments, so it can extend the summaries and descriptions
                c.OperationFilter<BasicAuthOperationsFilter>();
                c.ExampleFilters();
            });
            
            services.AddSwaggerExamplesFromAssemblies(typeof(Startup).Assembly);

            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuthentication", null);
            
            services.AddFsReleaseArtifactService(Configuration);

            services.AddCors(options => CorsPolicies.Configure(options, Configuration));

            // Explicitly use the Hellang middleware: the response format is part of the public API.
            ProblemDetailsExtensions.AddProblemDetails(services, LegacyProblemDetails.Configure);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Provides the mechanisms to configure the request pipeline of the application.</param>
        /// <param name="env">Information about the web hosting environment of the application.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseProblemDetails();

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;

                var result = JsonConvert.SerializeObject(new { error = exception.Message });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }));

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            //Register Swagger UI middleware & generator
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Release Server API V1");
            });
        }
        
        #endregion
    }
}
