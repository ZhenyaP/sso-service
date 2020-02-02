using System;
using System.IO;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityProvider.Common.Entities
{
    public class BaseStartup
    {
        /// <summary>
        /// The configuration.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        public IConfigurationSection ConfigSettingsSection => this.Configuration.GetSection(nameof(ConfigSettings));

        public RegionEndpoint GetRegionEndpoint()
        {
            var aws = new AWS();
            var awsSection = this.Configuration.GetSection(nameof(AWS));
            awsSection.Bind(aws);
            var regionEndpoint = RegionEndpoint.GetBySystemName(aws.Region);

            return regionEndpoint;
        }

        public void AddCognitoIdentityProviderService(IServiceCollection services,
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

        public Credentials GetCredentials()
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
    }
}
