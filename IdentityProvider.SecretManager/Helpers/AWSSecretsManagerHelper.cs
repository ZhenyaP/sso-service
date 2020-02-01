//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the AWSSecretsManagerHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        private ILogger<AWSSecretsManagerHelper> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSSecretsManagerHelper"/> class.
        /// </summary>
        public AWSSecretsManagerHelper(IAmazonSecretsManager amazonSecretsManager,
            ILogger<AWSSecretsManagerHelper> logger)
        {
            this._amazonSecretsManager = amazonSecretsManager;
            this._logger = logger;
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
