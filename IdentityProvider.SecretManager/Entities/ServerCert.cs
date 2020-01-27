//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the ServerCert type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.SecretManager.Entities
{
    /// <summary>
    /// Class ServerCert
    /// </summary>
    public class ServerCert
    {
        /// <summary>
        /// Gets or sets the private key.
        /// </summary>
        /// <value>The private key.</value>
        public SecretData PrivateKey { get; set; }

        /// <summary>
        /// Gets or sets the ARN.
        /// </summary>
        /// <value>The ARN.</value>
        public string Arn { get; set; }

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
