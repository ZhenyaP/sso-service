//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the CrlChain type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

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
        public CrlData RootCa { get; set; }

        /// <summary>
        /// Gets or sets the Intermediate CA CRL.
        /// </summary>
        /// <value>The Intermediate CA CRL.</value>
        public CrlData IntermediateCa { get; set; }

        /// <summary>
        /// Gets or sets the destination file name (in Docker Secrets volume).
        /// </summary>
        /// <value>The destination file name (in Docker Secrets volume).</value>
        public string DestinationFileName { get; set; }
    }
}
