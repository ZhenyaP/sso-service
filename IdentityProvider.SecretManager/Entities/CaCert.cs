//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the CaCert type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.SecretManager.Entities
{
    /// <summary>
    /// Class CaCert.
    /// </summary>
    public class CaCert
    {
        /// <summary>
        /// Gets or sets the PCA CA Cert ARN.
        /// </summary>
        /// <value>The PCA CA Cert ARN.</value>
        public string Arn { get; set; }

        /// <summary>
        /// Gets or sets the CA Cert Destination File Name (in Docker volume).
        /// </summary>
        /// <value>The CA Cert Destination File Name (in Docker volume).</value>
        public string DestinationFileName { get; set; }
    }
}
