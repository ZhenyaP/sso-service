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
        /// Gets or sets the Cognito client app.
        /// </summary>
        /// <value>The Cognito client app.</value>
        public CognitoClient ClientApp { get; set; }

        /// <summary>
        /// Gets or sets the token URL.
        /// </summary>
        /// <value>The token URL.</value>
        public string TokenUrl { get; set; }

        public string CognitoJwksUrl => $"{this.TokenIssuer}/.well-known/jwks.json";

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
