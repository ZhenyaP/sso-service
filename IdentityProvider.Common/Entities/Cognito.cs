//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the Cognito type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common.Entities
{
    /// <summary>
    /// The Cognito.
    /// </summary>
    public class Cognito
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>The client identifier.</value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        /// <value>The client secret.</value>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the user pool identifier.
        /// </summary>
        /// <value>The user pool identifier.</value>
        public string UserPoolId { get; set; }

        /// <summary>
        /// Gets or sets the token URL.
        /// </summary>
        /// <value>The token URL.</value>
        public string TokenUrl { get; set; }

        /// <summary>
        /// Gets or sets the Cognito token issuer.
        /// </summary>
        /// <value>The Cognito token issuer.</value>
        public string TokenIssuer { get; set; }

        /// <summary>
        /// Gets or sets the client credentials authentication scope.
        /// </summary>
        /// <value>The client credentials authentication scope.</value>
        public string ClientCredentialsAuthScope { get; set; }        
    }
}
