using IdentityProvider.Common.Entities;

namespace IdentityProvider.API.Entities
{
    using IdentityProvider.Common.Entities;

    /// <summary>
    /// The Config Settings.
    /// </summary>
    public class ConfigSettings
    {
        /// <summary>
        /// Gets or sets the Token Request Timespan (in seconds)
        /// </summary>
        /// <value>The Token Request Timespan (in seconds).</value>
        public int TokenRequestTimespanSecs { get; set; }

        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>The clients.</value>
        public ConfigClientData[] Clients { get; set; }

        /// <summary>
        /// Gets or sets the Secrets Docker volume Path.
        /// </summary>
        /// <value>The Secrets Docker volume Path.</value>
        public string SecretsDockerFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the Cognito Secrets File Name.
        /// </summary>
        /// <value>The Cognito Secrets File Name.</value>
        public string CognitoSecretsFileName { get; set; }

        /// <summary>
        /// Gets or sets the RsaKeyData File Name.
        /// </summary>
        /// <value>The Destination RsaKeyData File Name.</value>
        public string RsaKeyDataFileName { get; set; }
    }
}
