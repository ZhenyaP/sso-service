namespace IdentityProvider.Common.Enums
{
    public enum IdentityProvider
    {
        /// <summary>
        /// AWS Cognito
        /// </summary>
        Cognito,

        /// <summary>
        /// Google Firebase
        /// </summary>
        Firebase,

        /// <summary>
        /// The IdentityProvider.API application acts by itself as the final
        /// Identity Provider
        /// </summary>
        Self
    }
}
