using IdentityProvider.Common.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace IdentityProvider.API
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using Amazon;
    using Amazon.CognitoIdentityProvider;
    using Amazon.SecurityToken.Model;
    using Entities;
    using Common;
    using Common.Helpers;
    using Common.Middlewares;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Helpers;
    using Middlewares;
    using System.Reflection;

    /// <summary>
    /// Class Startup.
    /// </summary>
    public class Startup : BaseStartup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// This method gets called by the runtime.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMiddleware<AsyncInitializationMiddleware>();
            app.UseMiddleware<RequestLoggerMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve Swagger-UI (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Provider API V1");
            });

            app.UseMvc();
        }

        /// <summary>
        /// Adds services to the container.
        /// This method gets called by the runtime.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Latest);
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Identity Provider API",
                    Description = @"The Identity Provider (IdP) with API that authenticates 
                    users/authorizes clients and returns various tokens. 
                    This way this IdP federates multiple service providers 
                    (SaaS apps) and acts as the key part of the whole FIdM (Federated Identity Management) mechanism.",
                    //TermsOfService = "None",
                    Contact = new OpenApiContact
                    {
                        Name = "Eugene Petrovich",
                        Email = "zhpetrovich@yahoo.com",
                        Url = new Uri("https://www.linkedin.com/in/yauheniy-piatrovich/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license")
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddOptions();
            services.Configure<ConfigSettings>(this.Configuration.GetSection(nameof(ConfigSettings)));
            services.AddTransient<AWSCognitoHelper, AWSCognitoHelper>();
            AddCognitoIdentityProviderService(services, GetRegionEndpoint(), GetCredentials());

            services.AddSingleton(
                p =>
                {
                    var configSettings = p.GetService<IOptions<ConfigSettings>>().Value;
                    var httpClient = new HttpClient(new SocketsHttpHandler
                    {
                        MaxConnectionsPerServer = 100,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    })
                    {
                        Timeout = TimeSpan.FromSeconds(configSettings.TokenRequestTimespanSecs)
                    };

                    return httpClient;
                });
            services.AddSingleton<AWSCognitoClientSecretHelper, AWSCognitoClientSecretHelper>();
            services.AddSingleton<JwtTokenHelper, JwtTokenHelper>();
            services.AddSingleton<FirebaseHelper, FirebaseHelper>();
        }
    }
}
