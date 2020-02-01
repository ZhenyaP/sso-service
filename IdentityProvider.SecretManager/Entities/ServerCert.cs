//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the ServerCert type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using IdentityProvider.SecretManager.Entities.AWS;

namespace IdentityProvider.SecretManager.Entities
{
    /// <summary>
    /// Class ServerCert
    /// </summary>
    public class ServerCert
    {
        public ServerCertAWSConfig AWSConfig { get; set; }

        /// <summary>
        /// Gets or sets the Destination Cert File Name.
        /// </summary>
        /// <value>The Destination Cert File Name.</value>
        public string DestinationCertFileName { get; set; }

        /// <summary>
        /// Gets or sets the Destination Private Key File Name.
        /// </summary>
        /// <value>The Destination Private Key File Name.</value>
        public string DestinationPrivateKeyFileName { get; set; }
    }
}
