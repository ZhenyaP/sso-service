//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the ClientCert type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common.Entities
{
    /// <summary>
    /// The SSL Client Certificate.
    /// </summary>
    public class ClientCert
    {
        /// <summary>
        /// Gets or sets the Client Cert Subject Common Name (CN)
        /// </summary>
        /// <value>The Client Cert Subject Common Name (CN).</value>
        public string SubjectCommonName { get; set; }

        /// <summary>
        /// Gets or sets the Client Cert Issuer Common Name (CN)
        /// </summary>
        /// <value>The Client Cert Issuer Common Name (CN).</value>
        public string IssuerCommonName { get; set; }

        /// <summary>
        /// Gets or sets the Client Cert Serial Number
        /// </summary>
        /// <value>The Client Cert Serial Number.</value>
        public string SerialNumber { get; set; }
    }
}
