using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using ReleaseServer.WebApi.Auth;
using ReleaseServer.WebApi.Config;
using ReleaseServer.WebApi.Extensions;
using ReleaseServer.WebApi.JsonConverters;
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
    }
}
