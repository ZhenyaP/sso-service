//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the CrlChain type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using IdentityProvider.SecretManager.Entities.AWS;

namespace IdentityProvider.SecretManager.Entities
{
    /// <summary>
    /// Class CrlChain
    /// </summary>
    public class CrlChain
    {
        /// <summary>
        /// Gets or sets the Root CA CRL.
        /// </summary>
        /// <value>The Root CA CRL.</value>
        public CrlAWSConfig RootCaCrlAWSConfig { get; set; }

        /// <summary>
        /// Gets or sets the Intermediate CA CRL.
        /// </summary>
        /// <value>The Intermediate CA CRL.</value>
        public CrlAWSConfig IntermediateCaCrlAWSConfig { get; set; }

        /// <summary>
        /// Gets or sets the destination file name (in Docker Secrets volume).
        /// </summary>
        /// <value>The destination file name (in Docker Secrets volume).</value>
        public string DestinationFileName { get; set; }
    }
}
