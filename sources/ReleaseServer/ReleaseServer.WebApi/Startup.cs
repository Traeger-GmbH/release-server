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
using Microsoft.OpenApi.Models;

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
        /// <param name="env">Information about the web hosting environment of the application.</param>
        public Startup(IWebHostEnvironment env)
        {
            var conf = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .Build();
            
            Configuration = conf;
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
                });

            services.AddSwaggerGen(c =>
            {
                
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Release Server API",
                    Version = "v1",
                    Description = "An application for managing your own release artifacts. " +
                                  "The release server provides several REST endpoints for the following operations.",
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
                
                c.OperationFilter<BasicAuthOperationsFilter>();
                
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ReleaseServer.WebApi.xml"));
                c.ExampleFilters();
            });
            
            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuthentication", null);
            
            services.AddFsReleaseArtifactService(Configuration);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(_ => true)
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000));
                });
            });
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
