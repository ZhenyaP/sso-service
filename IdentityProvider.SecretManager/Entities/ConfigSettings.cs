//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the ConfigSettings type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using IdentityProvider.SecretManager.Entities.AWS;

namespace IdentityProvider.SecretManager.Entities
{
    /// <summary>
    /// Class ConfigSettings
    /// </summary>
    public class ConfigSettings
    {
        /// <summary>
        /// Gets or sets the Secret Provider.
        /// </summary>
        /// <value>The Secret Provider.</value>
        public SecretProvider SecretProvider { get; set; }
        
        /// <summary>
        /// Gets or sets the PCA CA Cert.
        /// </summary>
        /// <value>The PCA CA Cert.</value>
        public CaCert CaCert { get; set; }

        /// <summary>
        /// Gets or sets the PCA Server Domain Cert.
        /// </summary>
        /// <value>The PCA Server Domain Cert.</value>
        public ServerCert ServerCert { get; set; }

        /// <summary>
        /// Gets or sets the CRL Chain.
        /// </summary>
        /// <value>The CRL Chain.</value>
        public CrlChain CrlChain { get; set; }

        /// <summary>
        /// Gets or sets the Secrets Docker volume Path.
        /// </summary>
        /// <value>The Secrets Docker volume Path.</value>
        public string SecretsDockerFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the Extra AWS Config.
        /// </summary>
        /// <value>The Extra AWS Config.</value>
        public ExtraAWSConfig ExtraAWSConfig { get; set; }

        /// <summary>
        /// Gets or sets the RsaKeyData File Name.
        /// </summary>
        /// <value>The Destination RsaKeyData File Name.</value>
        public string RsaKeyDataFileName { get; set; }
    }
}
