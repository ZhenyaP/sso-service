//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the CognitoClientSecretData type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common.Entities
{
    /// <summary>
    /// The Cognito Client Secret Data.
    /// </summary>
    public class CognitoClientSecretData
    {
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>The client identifier.</value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the user pool identifier.
        /// </summary>
        /// <value>The user pool identifier.</value>
        public string UserPoolId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        /// <value>The client secret.</value>
        public string ClientSecret { get; set; }
    }
}
