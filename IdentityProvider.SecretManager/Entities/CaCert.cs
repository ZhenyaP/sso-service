//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the CaCert type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using IdentityProvider.SecretManager.Entities.AWS;

namespace IdentityProvider.SecretManager.Entities
{
    /// <summary>
    /// Class CaCert.
    /// </summary>
    public class CaCert
    {
        /// <summary>
        /// Gets or sets the CA Cert configuration for AWS.
        /// </summary>
        /// <value>The CA Cert configuration for AWS.</value>
        public CaCertAWSConfig AWSConfig { get; set; }

        /// <summary>
        /// Gets or sets the CA Cert Destination File Name (in Docker volume).
        /// </summary>
        /// <value>The CA Cert Destination File Name (in Docker volume).</value>
        public string DestinationFileName { get; set; }
    }
}
