using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.ACMPCA;
using Amazon.S3.Model;
using IdentityProvider.Common;
using IdentityProvider.Common.Helpers;
using IdentityProvider.SecretManager.Entities;
using IdentityProvider.SecretManager.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = ServiceStack.Text.JsonSerializer;

namespace IdentityProvider.SecretManager.Helpers
{
    public class AWSSecretHelper : ISecretHelper
    {
        /// <summary>
        /// The Config Settings
        /// </summary>
        private readonly ConfigSettings _configSettings;

        /// <summary>
        /// The AWS S3 Bucket Helper
        /// </summary>
        private readonly AWSS3BucketHelper _awss3BucketHelper;

        /// <summary>
        /// AWS Secrets Manager Helper
        /// </summary>
        private readonly AWSSecretsManagerHelper _awsSecretsManagerHelper;

        /// <summary>
        /// Amazon ACM PCA
        /// </summary>
        private readonly IAmazonACMPCA _amazonAcmPca;

        /// <summary>
        /// AWS Cognito Helper
        /// </summary>
        private readonly AWSCognitoHelper _awsCognitoHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSSecretHelper"/> class.
        /// </summary>
        /// <param name="configSettings">The Config Settings.</param>
        /// <param name="amazonAcmPca">The Amazon ACM PCA.</param>
        /// <param name="awss3BucketHelper">The AWS S3 Bucket Helper.</param>
        /// <param name="awsSecretsManagerHelper">The AWS Secrets Manager Helper.</param>
        /// <param name="awsCognitoHelper">The AWS Cognito Helper.</param>
        public AWSSecretHelper(IOptions<ConfigSettings> configSettings,
            IAmazonACMPCA amazonAcmPca,
            AWSS3BucketHelper awss3BucketHelper,
            AWSSecretsManagerHelper awsSecretsManagerHelper,
            AWSCognitoHelper awsCognitoHelper)
        {
            this._configSettings = configSettings.Value;
            this._amazonAcmPca = amazonAcmPca;
            this._awss3BucketHelper = awss3BucketHelper;
            this._awsSecretsManagerHelper = awsSecretsManagerHelper;
            this._awsCognitoHelper = awsCognitoHelper;
        }

        public async Task CreateChainCrlFileAsync()
        {
            var derEncodedIntermediateCaCrlBytesContent = await this._awss3BucketHelper.GetObjectContentAsync<byte[]>(
                new GetObjectRequest
                {
                    BucketName = this._configSettings.CrlChain.IntermediateCaCrlAWSConfig.BucketName,
                    Key = this._configSettings.CrlChain.IntermediateCaCrlAWSConfig.Key
                }).ConfigureAwait(false);
            var pemEncodedIntermediateCaCrlContent = CertHelper.ConvertDerEncodedCrlToPem(derEncodedIntermediateCaCrlBytesContent);

            var pemEncodedRootCaCrlContent = await this._awss3BucketHelper.GetObjectContentAsync<string>(
                new GetObjectRequest
                {
                    BucketName = this._configSettings.CrlChain.RootCaCrlAWSConfig.BucketName,
                    Key = this._configSettings.CrlChain.RootCaCrlAWSConfig.Key
                }).ConfigureAwait(false);
            var chainCrlContent = pemEncodedIntermediateCaCrlContent + pemEncodedRootCaCrlContent;
            await FileHelper.CreateFileWithContentAsync(Path.Combine(this._configSettings.SecretsDockerFolderPath,
                    this._configSettings.CrlChain.DestinationFileName), chainCrlContent)
                .ConfigureAwait(false);
        }

        public async Task CreateCaChainFileAsync()
        {
            var caCert = await this._amazonAcmPca.GetCertificateAuthorityCertificateAsync(
                new Amazon.ACMPCA.Model.GetCertificateAuthorityCertificateRequest
                {
                    CertificateAuthorityArn = this._configSettings.CaCert.AWSConfig.Arn
                }).ConfigureAwait(false);

            var caIntermediateCertContent = caCert.Certificate;
            if (!caIntermediateCertContent.EndsWith(Environment.NewLine))
            {
                caIntermediateCertContent += Environment.NewLine;
            }

            var caRootCertContent = caCert.CertificateChain;
            if (!caRootCertContent.EndsWith(Environment.NewLine))
            {
                caRootCertContent += Environment.NewLine;
            }

            var caChainCertContent = caIntermediateCertContent + caRootCertContent;
            await FileHelper.CreateFileWithContentAsync(
                    Path.Combine(this._configSettings.SecretsDockerFolderPath,
                        this._configSettings.CaCert.DestinationFileName), caChainCertContent)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Constructs the valid cert's private key string content.
        /// </summary>
        /// <param name="originalPrivateKey">The private key string
        /// content that was obtained from AWS Secrets Manager.</param>
        /// <returns>The valid cert's private key string content.</returns>
        private string ConstructValidPrivateKey(string originalPrivateKey)
        {
            var privateKeyBody =
                originalPrivateKey.Substring(CommonConstants.Cert.RsaPrivateKeyHeader.Length);
            privateKeyBody = privateKeyBody.Substring(0, privateKeyBody.Length - CommonConstants.Cert.RsaPrivateKeyFooter.Length);
            var validPrivateKey = string.Concat(CommonConstants.Cert.RsaPrivateKeyHeader,
                privateKeyBody.Replace(" ", Environment.NewLine),
                CommonConstants.Cert.RsaPrivateKeyFooter,
                Environment.NewLine);

            return validPrivateKey;
        }

        public async Task CreateServerCertFilesAsync()
        {
            var serverCert = await this._amazonAcmPca.GetCertificateAsync(
                new Amazon.ACMPCA.Model.GetCertificateRequest
                {
                    CertificateArn = string.Format(_configSettings.ServerCert.AWSConfig.Arn,
                        _configSettings.CaCert.AWSConfig.Arn),
                    CertificateAuthorityArn = _configSettings.CaCert.AWSConfig.Arn
                }).ConfigureAwait(false);
            await FileHelper.CreateFileWithContentAsync(
                    Path.Combine(this._configSettings.SecretsDockerFolderPath,
                        this._configSettings.ServerCert.DestinationCertFileName), serverCert.Certificate)
                .ConfigureAwait(false);

            var serverCertPrivateKey = await this._awsSecretsManagerHelper.GetSecretValueAsync(
                this._configSettings.ServerCert.AWSConfig.PrivateKey.SecretName,
                this._configSettings.ServerCert.AWSConfig.PrivateKey.SecretKey).ConfigureAwait(false);
            var validServerCertPrivateKey = ConstructValidPrivateKey(serverCertPrivateKey);
            await FileHelper.CreateFileWithContentAsync(
                    Path.Combine(this._configSettings.SecretsDockerFolderPath,
                        this._configSettings.ServerCert.DestinationPrivateKeyFileName), validServerCertPrivateKey)
                .ConfigureAwait(false);

            var rsaKeyData = CertHelper.GetRsaKeyData(validServerCertPrivateKey);
            await FileHelper.CreateFileWithContentAsync(Path.Combine(this._configSettings.SecretsDockerFolderPath,
                    this._configSettings.RsaKeyDataFileName), JsonConvert.SerializeObject(rsaKeyData))
                .ConfigureAwait(false);
        }

        private async Task CreateCognitoSecretFileAsync()
        {
            foreach (var cognitoClient in this._configSettings.ExtraAWSConfig.CognitoClients)
            {
                cognitoClient.ClientSecret = await _awsCognitoHelper.GetClientSecretForAppClientAsync(cognitoClient)
                    .ConfigureAwait(false);
            }

            await FileHelper.CreateFileWithContentAsync(Path.Combine(this._configSettings.SecretsDockerFolderPath,
                    this._configSettings.ExtraAWSConfig.CognitoSecretsFileName),
                JsonSerializer.SerializeToString(this._configSettings.ExtraAWSConfig.CognitoClients))
                .ConfigureAwait(false);
        }

        public async Task CreateThirdPartyIdentityProvidersSecretsFilesAsync()
        {
            await CreateCognitoSecretFileAsync();
        }
    }
}
