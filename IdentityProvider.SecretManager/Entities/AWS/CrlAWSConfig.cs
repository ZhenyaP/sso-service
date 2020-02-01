namespace IdentityProvider.SecretManager.Entities.AWS
{
    public class CrlAWSConfig
    {
        /// <summary>
        /// Gets or sets the AWS S3 Bucket Name.
        /// </summary>
        /// <value>The AWS S3 Bucket Name.</value>
        public string BucketName { get; set; }

        /// <summary>
        /// Gets or sets the AWS S3 Bucket Key.
        /// </summary>
        /// <value>The AWS S3 Bucket Key.</value>
        public string Key { get; set; }
    }
}
