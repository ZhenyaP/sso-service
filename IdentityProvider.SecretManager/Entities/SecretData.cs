//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the Passphrase type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.SecretManager.Entities
{
    /// <summary>
    /// Class SecretData
    /// </summary>
    public class SecretData
    {
        /// <summary>
        /// Gets or sets the Secret Name.
        /// </summary>
        /// <value>The Secret Name.</value>
        public string SecretName { get; set; }

        /// <summary>
        /// Gets or sets the Secret Key.
        /// </summary>
        /// <value>The Secret Key.</value>
        public string SecretKey { get; set; }
    }
}
