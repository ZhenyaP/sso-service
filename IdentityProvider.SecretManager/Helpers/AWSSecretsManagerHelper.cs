//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the AWSSecretsManagerHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using Serilog;

namespace IdentityProvider.SecretManager.Helpers
{
    /// <summary>
    /// The AWS Secrets Manager Helper.
    /// </summary>
    public class AWSSecretsManagerHelper
    {
        /// <summary>
        /// The Amazon Secrets Manager
        /// </summary>
        private readonly IAmazonSecretsManager _amazonSecretsManager;

        /// <summary>
        /// The log
        /// </summary>
        private static readonly ILogger Log = Serilog.Log.ForContext<AWSSecretsManagerHelper>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSSecretsManagerHelper"/> class.
        /// </summary>
        public AWSSecretsManagerHelper(IAmazonSecretsManager amazonSecretsManager/*,
            IOptions<ConfigSettings> configSettings*/)
        {
            this._amazonSecretsManager = amazonSecretsManager;
            //new AmazonSecretsManagerConfig {ServiceURL = ""};
            //MockingHelper.SetPropertyValue(_amazonSecretsManager.Config.RegionEndpoint, "RegionEndpoint", null);
            //var serviceUrl = new UriBuilder(Uri.UriSchemeHttps, configSettings.Value.SecretsManagerVpceDnsName).Uri.AbsoluteUri;
            //Log.Information($"AWSSecretsManagerHelper: serviceUrl={serviceUrl}");
            //MockingHelper.SetPropertyValue(_amazonSecretsManager.Config, "ServiceURL", serviceUrl);
        }

        /// <summary>
        /// Gets the Secret Value
        /// </summary>
        /// <param name="secretName">The Secret Name</param>
        /// <param name="secretKey">The Secret Key</param>
        /// <returns>The Secret Value</returns>
        public async Task<string> GetSecretValueAsync(string secretName, string secretKey)
        {
            var getSecretValueRequest =
                new GetSecretValueRequest { SecretId = secretName };
            var secretValueResponse = await this._amazonSecretsManager.GetSecretValueAsync(getSecretValueRequest)
                .ConfigureAwait(false);
            var secretDictionary =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(secretValueResponse.SecretString);

            return secretDictionary[secretKey];
        }
    }
}
