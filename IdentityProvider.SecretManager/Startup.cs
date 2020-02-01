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
using IdentityProvider.SecretManager.Entities.AWS;
using IdentityProvider.SecretManager.Helpers;
using IdentityProvider.SecretManager.Helpers.Interfaces;
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

        #region Private

        private IConfigurationSection ConfigSettingsSection => this.Configuration.GetSection(nameof(ConfigSettings));

        private RegionEndpoint GetRegionEndpoint()
        {
            var aws = new AWS();
            var awsSection = this.Configuration.GetSection(nameof(AWS));
            awsSection.Bind(aws);
            var regionEndpoint = RegionEndpoint.GetBySystemName(aws.Region);

            return regionEndpoint;
        }

        private void AddS3Service(IServiceCollection services,
            RegionEndpoint regionEndpoint,
            Credentials credentials)
        {
            services.AddSingleton<IAmazonS3>(
                p =>
                {
                    var config = new AmazonS3Config
                    {
                        RegionEndpoint = regionEndpoint
                    };
                    return credentials == null
                        ? new AmazonS3Client(config) :
                        new AmazonS3Client(credentials, config);
                });
        }

        private ConfigSettings GetConfigSettings()
        {
            var configSettings = new ConfigSettings();
            ConfigSettingsSection.Bind(configSettings);

            return configSettings;
        }

        private string GetSecretsManagerServiceUrl(IServiceCollection services)
        {
            var serviceUrl = new UriBuilder(Uri.UriSchemeHttps,
                GetConfigSettings().ExtraAWSConfig.SecretsManagerVpceDnsName).Uri.AbsoluteUri;

            return serviceUrl;
        }

        private void AddSecretManagerService(IServiceCollection services,
            RegionEndpoint regionEndpoint,
            Credentials credentials)
        {
            services.AddSingleton<IAmazonSecretsManager>(
                p =>
                {
                    var config = new AmazonSecretsManagerConfig
                    {
                        ServiceURL = GetSecretsManagerServiceUrl(services),
                        RegionEndpoint = regionEndpoint
                    };
                    return credentials == null
                        ? new AmazonSecretsManagerClient(config) :
                    new AmazonSecretsManagerClient(
                        credentials,
                        config);
                });
        }

        private void AddACMPCAService(IServiceCollection services,
            RegionEndpoint regionEndpoint,
            Credentials credentials)
        {
            services.AddSingleton<IAmazonACMPCA>(
                p =>
                {
                    var config = new AmazonACMPCAConfig
                    {
                        RegionEndpoint = regionEndpoint
                    };
                    return credentials == null ?
                        new AmazonACMPCAClient(config) :
                        new AmazonACMPCAClient(credentials, config);
                });
        }

        private void AddCognitoIdentityProviderService(IServiceCollection services,
            RegionEndpoint regionEndpoint,
            Credentials credentials)
        {
            services.AddSingleton<IAmazonCognitoIdentityProvider>(
                p =>
                {
                    var config = new AmazonCognitoIdentityProviderConfig
                    {
                        RegionEndpoint = regionEndpoint
                    };
                    return credentials == null ?
                    new AmazonCognitoIdentityProviderClient(config) :
                    new AmazonCognitoIdentityProviderClient(
                        credentials, config);
                });
        }

        private Credentials GetCredentials()
        {
            Credentials credentials = null;
            if (File.Exists(CommonConstants.AwsKeysFileName))
            {
                var awsKeysData = File.ReadAllLines(CommonConstants.AwsKeysFileName);
                var accessKeyId = awsKeysData[0];
                var secretAccessKey = awsKeysData[1];
                var sessionToken = awsKeysData[2];
                credentials = new Credentials(
                    accessKeyId,
                    secretAccessKey,
                    sessionToken,
                    DateTime.UtcNow.AddDays(1));
            }

            return credentials;
        }

        #endregion

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
            services.Configure<ConfigSettings>(ConfigSettingsSection);

            var configSettings = GetConfigSettings();
            if (configSettings.SecretProvider == SecretProvider.AWS)
            {
                var credentials = GetCredentials();
                var regionEndpoint = GetRegionEndpoint();

                AddS3Service(services, regionEndpoint, credentials);
                AddSecretManagerService(services, regionEndpoint, credentials);
                AddACMPCAService(services, regionEndpoint, credentials);
                AddCognitoIdentityProviderService(services, regionEndpoint, credentials);

                services.AddTransient<AWSCognitoHelper, AWSCognitoHelper>();
                services.AddTransient<AWSS3BucketHelper, AWSS3BucketHelper>();
                services.AddTransient<AWSSecretsManagerHelper, AWSSecretsManagerHelper>();
                services.AddTransient<ISecretHelper, AWSSecretHelper>();
            }

            services.AddSingleton(p => new HttpClient(new SocketsHttpHandler
            {
                MaxConnectionsPerServer = 100,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            }));

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
