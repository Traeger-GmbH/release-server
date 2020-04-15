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
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NLog.Fluent;
using ReleaseServer.WebApi.Auth;
using ReleaseServer.WebApi.Common;
using ReleaseServer.WebApi.SwaggerDocu;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var conf = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .Build();
            
            Configuration = conf;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

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
    }
}