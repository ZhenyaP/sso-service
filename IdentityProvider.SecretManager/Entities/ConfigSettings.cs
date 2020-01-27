﻿//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the ConfigSettings type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using IdentityProvider.Common.Entities;

namespace IdentityProvider.SecretManager.Entities
{
    /// <summary>
    /// Class ConfigSettings
    /// </summary>
    public class ConfigSettings
    {
        /// <summary>
        /// Gets or sets the PCA CA Cert.
        /// </summary>
        /// <value>The PCA CA Cert.</value>
        public CaCert CaCert { get; set; }

        /// <summary>
        /// Gets or sets the PCA Server Domain Cert.
        /// </summary>
        /// <value>The PCA Server Domain Cert.</value>
        public ServerCert ServerCert { get; set; }

        /// <summary>
        /// Gets or sets the CRL Chain.
        /// </summary>
        /// <value>The CRL Chain.</value>
        public CrlChain CrlChain { get; set; }

        /// <summary>
        /// Gets or sets the Secrets Docker volume Path.
        /// </summary>
        /// <value>The Secrets Docker volume Path.</value>
        public string SecretsDockerFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the Cognito clients.
        /// </summary>
        /// <value>The Cognito clients.</value>
        public CognitoClientSecretData[] CognitoClients { get; set; }

        /// <summary>
        /// Gets or sets the Destination Cognito Secrets File Name.
        /// </summary>
        /// <value>The Destination Cognito Secrets File Name.</value>
        public string CognitoSecretsFileName { get; set; }

        /// <summary>
        /// Gets or sets the RsaKeyData File Name.
        /// </summary>
        /// <value>The Destination RsaKeyData File Name.</value>
        public string RsaKeyDataFileName { get; set; }

        /// <summary>
        /// Gets or sets the VPC endpoint-specific DNS hostname for AWS Secrets Manager service.
        /// </summary>
        /// <value>The VPC endpoint-specific DNS hostname for AWS Secrets Manager service.</value>
        public string SecretsManagerVpceDnsName { get; set; }
    }
}
