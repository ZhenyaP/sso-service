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
        /// The collection of the Cognito Clients.
        /// </summary>
        private static CognitoClient[] _cognitoClients;

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
        /// Gets the collection of the Cognito Clients.
        /// </summary>
        public CognitoClient[] CognitoClients
        {
            get
            {   //Thread-safe setting cache data
                if (_cognitoClients == null)
                {
                    var cognitoSecretsText = File.ReadAllText(Path.Combine(
                                this._configSettings.SecretsDockerFolderPath,
                                this._configSettings.CognitoSecretsFileName));

                    _cognitoClients = JsonSerializer.DeserializeFromString<CognitoClient[]>(cognitoSecretsText);
                }

                return _cognitoClients;
            }
        }

        /// <summary>
        /// Gets the Client Secret For Current Client
        /// </summary>
        /// <param name="client">The Client</param>
        /// <returns>The Client Secret</returns>
        public string GetClientSecretForCognitoClient(ConfigClientData client)
        {
            var clientSecret = CognitoClients.FirstOrDefault(x => x.UserPoolId == client.Cognito.ClientApp.UserPoolId &&
                                                                              x.ClientId == client.Cognito.ClientApp.ClientId)?.ClientSecret;

            return clientSecret;
        }
    }
}
