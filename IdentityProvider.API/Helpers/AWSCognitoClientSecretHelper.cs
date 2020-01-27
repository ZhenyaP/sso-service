namespace IdentityProvider.API.Helpers
{
    using System.IO;
    using System.Linq;
    using Entities;
    using IdentityProvider.Common.Entities;
    using Microsoft.Extensions.Options;
    using ServiceStack.Text;

    /// <summary>
    /// The AWS Cognito Client Secret Helper. 
    /// </summary>
    public class AWSCognitoClientSecretHelper
    {
        /// <summary>
        /// The collection of the Cognito Client Secret Data.
        /// </summary>
        private static CognitoClientSecretData[] _cognitoClientSecretDataArr;

        /// <summary>
        /// The config settings
        /// </summary>
        private readonly ConfigSettings _configSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSCognitoClientSecretHelper"/> class.
        /// </summary>
        /// <param name="configSettings">The Config Settings.</param>
        public AWSCognitoClientSecretHelper(IOptions<ConfigSettings> configSettings)
        {
            _configSettings = configSettings.Value;
        }

        /// <summary>
        /// Gets the collection of the Cognito Client Secret Data.
        /// </summary>
        public CognitoClientSecretData[] CognitoClientSecretDataArr
        {
            get
            {   //Thread-safe setting cache data
                if (_cognitoClientSecretDataArr == null)
                {
                    var cognitoSecretsText = File.ReadAllText(Path.Combine(
                                this._configSettings.SecretsDockerFolderPath,
                                this._configSettings.CognitoSecretsFileName));

                    _cognitoClientSecretDataArr = JsonSerializer.DeserializeFromString<CognitoClientSecretData[]>(cognitoSecretsText);
                }

                return _cognitoClientSecretDataArr;
            }
        }

        /// <summary>
        /// Gets the Client Secret For Current Client
        /// </summary>
        /// <param name="client">The Client</param>
        /// <returns>The Client Secret</returns>
        public string GetClientSecretForCognitoClient(ConfigClientData client)
        {
            var clientSecret = CognitoClientSecretDataArr.FirstOrDefault(x => x.UserPoolId == client.Cognito.UserPoolId &&
                                                                              x.ClientId == client.Cognito.ClientId)?.ClientSecret;

            return clientSecret;
        }
    }
}
