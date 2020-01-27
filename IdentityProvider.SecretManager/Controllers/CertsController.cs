//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the CertsController type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.ACMPCA;
using Amazon.CertificateManager;
using Amazon.S3.Model;
using IdentityProvider.SecretManager.Entities;
using IdentityProvider.Common;
using IdentityProvider.Common.Helpers;
using IdentityProvider.SecretManager.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = ServiceStack.Text.JsonSerializer;

namespace IdentityProvider.SecretManager.Controllers
{
    /// <summary>
    /// Class CertsController.
    /// </summary>
    [Route("api/[controller]")]
    public class CertsController : ControllerBase
    {
        /// <summary>
        /// The Config Settings
        /// </summary>
        private readonly ConfigSettings _configSettings;

        /// <summary>
        /// The Amazon Certificate Manager
        /// </summary>
        private readonly IAmazonCertificateManager _amazonCertificateManager;

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
        /// Initializes a new instance of the <see cref="CertsController"/> class.
        /// </summary>
        /// <param name="configSettings">The Config Settings.</param>
        /// <param name="amazonAcmPca">The Amazon ACM PCA.</param>
        /// <param name="amazonCertificateManager">The Amazon Certificate Manager.</param>        
        /// <param name="awss3BucketHelper">The AWS S3 Bucket Helper.</param>
        /// <param name="awsSecretsManagerHelper">The AWS Secrets Manager Helper.</param>
        /// <param name="awsCognitoHelper">The AWS Cognito Helper.</param>
        public CertsController(IOptions<ConfigSettings> configSettings,
                               IAmazonACMPCA amazonAcmPca,
                               IAmazonCertificateManager amazonCertificateManager,
                               AWSS3BucketHelper awss3BucketHelper,
                               AWSSecretsManagerHelper awsSecretsManagerHelper,
                               AWSCognitoHelper awsCognitoHelper)
        {
            this._configSettings = configSettings.Value;
            this._amazonAcmPca = amazonAcmPca;
            this._amazonCertificateManager = amazonCertificateManager;
            this._awss3BucketHelper = awss3BucketHelper;
            this._awsSecretsManagerHelper = awsSecretsManagerHelper;
            this._awsCognitoHelper = awsCognitoHelper;
        }

        private async Task CreateChainCrlFileAsync()
        {
            var derEncodedIntermediateCaCrlBytesContent = await this._awss3BucketHelper.GetObjectContentAsync<byte[]>(
                new GetObjectRequest
                {
                    BucketName = this._configSettings.CrlChain.IntermediateCa.BucketName,
                    Key = this._configSettings.CrlChain.IntermediateCa.Key
                }).ConfigureAwait(false);
            var pemEncodedIntermediateCaCrlContent = CertHelper.ConvertDerEncodedCrlToPem(derEncodedIntermediateCaCrlBytesContent);

            var pemEncodedRootCaCrlContent = await this._awss3BucketHelper.GetObjectContentAsync<string>(
                new GetObjectRequest
                {
                    BucketName = this._configSettings.CrlChain.RootCa.BucketName,
                    Key = this._configSettings.CrlChain.RootCa.Key
                }).ConfigureAwait(false);
            var chainCrlContent = pemEncodedIntermediateCaCrlContent + pemEncodedRootCaCrlContent;
            await FileHelper.CreateFileWithContentAsync(Path.Combine(this._configSettings.SecretsDockerFolderPath,
                this._configSettings.CrlChain.DestinationFileName), chainCrlContent)
                .ConfigureAwait(false);
        }

        private async Task CreateCaChainFileAsync()
        {
            var caCert = await this._amazonAcmPca.GetCertificateAuthorityCertificateAsync(
                new Amazon.ACMPCA.Model.GetCertificateAuthorityCertificateRequest
                {
                    CertificateAuthorityArn = this._configSettings.CaCert.Arn
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

        private async Task CreateServerCertFilesAsync()
        {
            var serverCert = await this._amazonAcmPca.GetCertificateAsync(
                new Amazon.ACMPCA.Model.GetCertificateRequest
                {
                    CertificateArn = string.Format(_configSettings.ServerCert.Arn, _configSettings.CaCert.Arn),
                    CertificateAuthorityArn = _configSettings.CaCert.Arn
                }).ConfigureAwait(false);
            await FileHelper.CreateFileWithContentAsync(
                Path.Combine(this._configSettings.SecretsDockerFolderPath,
                    this._configSettings.ServerCert.DestinationCertFileName), serverCert.Certificate)
                .ConfigureAwait(false);

            var serverCertPrivateKey = await this._awsSecretsManagerHelper.GetSecretValueAsync(
                    this._configSettings.ServerCert.PrivateKey.SecretName,
                    this._configSettings.ServerCert.PrivateKey.SecretKey).ConfigureAwait(false);
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

        private async Task CreateCognitoSecretsFileAsync()
        {
            foreach (var cognitoClient in this._configSettings.CognitoClients)
            {
                cognitoClient.ClientSecret = await _awsCognitoHelper.GetClientSecretForAppClientAsync(cognitoClient)
                    .ConfigureAwait(false);
            }

            await FileHelper.CreateFileWithContentAsync(Path.Combine(this._configSettings.SecretsDockerFolderPath,
                    this._configSettings.CognitoSecretsFileName),
                JsonSerializer.SerializeToString(this._configSettings.CognitoClients)).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads Certs To Docker
        /// </summary>
        [HttpPost("Upload")]
        public async Task UploadCertsToDocker()
        {
            await Task.WhenAll(CreateChainCrlFileAsync(), CreateCaChainFileAsync(),
                CreateServerCertFilesAsync(), CreateCognitoSecretsFileAsync())
                .ConfigureAwait(false);
        }
    }
}
