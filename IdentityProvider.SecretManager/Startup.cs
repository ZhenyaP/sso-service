//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the Startup type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Amazon;
using Amazon.ACMPCA;
using Amazon.CertificateManager;
using Amazon.CognitoIdentityProvider;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SecurityToken.Model;
using IdentityProvider.SecretManager.Entities;
using IdentityProvider.Common;
using IdentityProvider.Common.Helpers;
using IdentityProvider.Common.Middlewares;
using IdentityProvider.SecretManager.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityProvider.SecretManager
{
    /// <summary>
    /// Class Startup.
    /// </summary>
    public class Startup
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
        /// The configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime.
        /// This method is used to add services to the container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service provider.</returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.AddOptions();
            var configSettingsSection = this.Configuration.GetSection(nameof(ConfigSettings));
            services.Configure<ConfigSettings>(configSettingsSection);
            if (File.Exists(CommonConstants.AwsKeysFileName))
            {
                var awsKeysData = File.ReadAllLines(CommonConstants.AwsKeysFileName);
                var accessKeyId = awsKeysData[0];
                var secretAccessKey = awsKeysData[1];
                var sessionToken = awsKeysData[2];

                services.AddSingleton<IAmazonS3>(
                    p => new AmazonS3Client(
                        new Credentials(
                            accessKeyId,
                            secretAccessKey,
                            sessionToken,
                            DateTime.UtcNow.AddDays(1)),
                        new AmazonS3Config
                        {
                            RegionEndpoint = RegionEndpoint.USEast1
                        }));
                services.AddSingleton<IAmazonSecretsManager>(
                    p => new AmazonSecretsManagerClient(
                        new Credentials(
                            accessKeyId,
                            secretAccessKey,
                            sessionToken,
                            DateTime.UtcNow.AddDays(1)),
                        new AmazonSecretsManagerConfig
                        {
                            RegionEndpoint = RegionEndpoint.USEast1
                        }));
                services.AddSingleton<IAmazonCertificateManager>(
                    p => new AmazonCertificateManagerClient(
                        new Credentials(
                            accessKeyId,
                            secretAccessKey,
                            sessionToken,
                            DateTime.UtcNow.AddDays(1)),
                        new AmazonCertificateManagerConfig
                        {
                            RegionEndpoint = RegionEndpoint.USEast1
                        }));
                services.AddSingleton<IAmazonACMPCA>(
                    p => new AmazonACMPCAClient(
                        new Credentials(
                            accessKeyId,
                            secretAccessKey,
                            sessionToken,
                            DateTime.UtcNow.AddDays(1)),
                        new AmazonACMPCAConfig
                        {
                            RegionEndpoint = RegionEndpoint.USEast1
                        }));
                services.AddSingleton<IAmazonCognitoIdentityProvider>(
                    p =>
                    {
                        var client = new AmazonCognitoIdentityProviderClient(
                            new Credentials(
                                accessKeyId,
                                secretAccessKey,
                                sessionToken,
                                DateTime.UtcNow.AddDays(1)),
                            new AmazonCognitoIdentityProviderConfig
                            {
                                RegionEndpoint = RegionEndpoint.USEast1
                            });
                        return client;
                    });
            }
            else
            {
                services.AddAWSService<IAmazonS3>();
                var configSettings = new ConfigSettings();
                configSettingsSection.Bind(configSettings);
                var serviceUrl = new UriBuilder(Uri.UriSchemeHttps, configSettings.SecretsManagerVpceDnsName).Uri.AbsoluteUri;
                services.AddSingleton<IAmazonSecretsManager>(p =>
                    new AmazonSecretsManagerClient(new AmazonSecretsManagerConfig
                    {
                        ServiceURL = serviceUrl,
                        RegionEndpoint = RegionEndpoint.USEast1
                    }));
                services.AddAWSService<IAmazonCertificateManager>();
                services.AddAWSService<IAmazonACMPCA>();
                services.AddAWSService<IAmazonCognitoIdentityProvider>();
            }

            services.AddSingleton(p => new HttpClient(new SocketsHttpHandler
            {
                MaxConnectionsPerServer = 100,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            }));
            services.AddTransient<AWSCognitoHelper, AWSCognitoHelper>();
            services.AddTransient<AWSS3BucketHelper, AWSS3BucketHelper>();
            services.AddTransient<AWSSecretsManagerHelper, AWSSecretsManagerHelper>();
            return services.BuildServiceProvider(true);
        }

        /// <summary>
        /// This method gets called by the runtime.
        /// This method is used to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The app</param>
        /// <param name="env">The hosting environment</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<RequestLoggerMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
