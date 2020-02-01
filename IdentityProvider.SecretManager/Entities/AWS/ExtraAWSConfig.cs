using IdentityProvider.Common.Entities;

namespace IdentityProvider.SecretManager.Entities.AWS
{
    public class ExtraAWSConfig
    {
        /// <summary>
        /// Gets or sets the Cognito clients.
        /// </summary>
        /// <value>The Cognito clients.</value>
        public CognitoClient[] CognitoClients { get; set; }

        /// <summary>
        /// Gets or sets the Destination Cognito Secrets File Name.
        /// </summary>
        /// <value>The Destination Cognito Secrets File Name.</value>
        public string CognitoSecretsFileName { get; set; }


        /// <summary>
        /// Gets or sets the VPC endpoint-specific DNS hostname for AWS Secrets Manager service.
        /// </summary>
        /// <value>The VPC endpoint-specific DNS hostname for AWS Secrets Manager service.</value>
        public string SecretsManagerVpceDnsName { get; set; }
    }
}
