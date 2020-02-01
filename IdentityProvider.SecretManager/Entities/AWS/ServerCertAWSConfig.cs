namespace IdentityProvider.SecretManager.Entities.AWS
{
    public class ServerCertAWSConfig
    {
        /// <summary>
        /// Gets or sets the private key.
        /// </summary>
        /// <value>The private key.</value>
        public AWSSecretData PrivateKey { get; set; }

        /// <summary>
        /// Gets or sets the ARN.
        /// </summary>
        /// <value>The ARN.</value>
        public string Arn { get; set; }
    }
}
