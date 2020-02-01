//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the Client type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common.Entities
{
    /// <summary>
    /// The Configuration Client Data.
    /// </summary>
    public class ConfigClientData
    {
        /// <summary>
        /// Gets or sets the Identity Provider.
        /// </summary>
        /// <value>The Identity Provider.</value>
        public Enums.IdentityProvider IdentityProvider { get; set; }

        public Enums.DigitalSignatureAlgorithm DigitalSignatureAlgorithm { get; set; }

        /// <summary>
        /// The SSL Client Certificate.
        /// </summary>
        public ClientCert ClientCert { get; set; }

        /// <summary>
        /// The Cognito.
        /// </summary>
        public Cognito Cognito { get; set; }

        /// <summary>
        /// The Firebase.
        /// </summary>
        public Firebase Firebase { get; set; }
    }
}
