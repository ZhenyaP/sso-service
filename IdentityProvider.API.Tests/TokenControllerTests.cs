using System.Linq;
using System.Net.Http;
using Amazon.CognitoIdentityProvider;
using IdentityProvider.Common.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace IdentityProvider.API.Tests
{
    using System.Threading.Tasks;
    using Common;
    using System;
    using Controllers;
    using Entities;
    using Helpers;
    using IdentityProvider.Common.Entities;
    using Newtonsoft.Json.Linq;
    using Xunit;

    /// <summary>
    /// The Token Controller Tests.
    /// </summary>
    public class TokenControllerTests
    {
        private const string username = "Eugene";

        /// <summary>
        /// Verifies the token/Session ID returned by the GetTokenWithSessionId
        /// action method from the TokenController.
        /// This test method violates main best practices of writing Unit Tests
        /// just for boosting the "unit" test performance.
        /// One more point: indeed, this is the Pseudo-Unit Test, as we are
        /// testing here the AWS Cognito Token obtained from External Service
        /// (AWS Cognito User Pool service). That's why it is more correct to
        /// call it an integration test.
        /// </summary>
        [Fact]
        public async Task GetTokenWithSessionId_VerifyTokenAndSessionId()
        {
            var configSettings = Options.Create(GetTestConfigSettings());
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(configSettings.Value.TokenRequestTimespanSecs)
            };
            var amazonCognitoIdentityProviderClient = new AmazonCognitoIdentityProviderClient();
            var awsCognitoHelper = new AWSCognitoHelper(httpClient, amazonCognitoIdentityProviderClient);
            var awsCognitoClientSecretHelper = new AWSCognitoClientSecretHelper(configSettings);
            var fireBaseHelper = new FirebaseHelper();
            var jwtTokenHelper = new JwtTokenHelper(configSettings, httpClient);

            var client = configSettings.Value.Clients.First();
            var cognitoClientSecretData = new CognitoClient
            {
                ClientId = client.Cognito.ClientApp.ClientId,
                UserPoolId = client.Cognito.ClientApp.UserPoolId
            };
            MockingHelper.SetFieldValue(awsCognitoClientSecretHelper, "_cognitoClientSecretDataArr", new[] { cognitoClientSecretData });
            cognitoClientSecretData.ClientSecret = await awsCognitoHelper.GetClientSecretForAppClientAsync(cognitoClientSecretData);
            var logger = Mock.Of<ILogger<TokenController>>();
            var controller = new TokenController(configSettings,
                logger,
                awsCognitoHelper,
                awsCognitoClientSecretHelper,
                fireBaseHelper,
                jwtTokenHelper)
            {
                CurrentClient = new Client
                {
                    ConfigClientData = client,
                    ExtraClientData = new ExtraClientData()
                }
            };
            controller.CurrentClient.ConfigClientData.Cognito.ClientApp.ClientSecret = cognitoClientSecretData.ClientSecret;
            await TestIdToken(true, controller, jwtTokenHelper).ConfigureAwait(false);
        }

        private async Task TestIdToken(bool isCognito,
            TokenController controller,
            JwtTokenHelper jwtTokenHelper)
        {
            controller.CurrentClient.ExtraClientData.UserName = username;
            if (!isCognito)
            {
                controller.CurrentClient.ExtraClientData.SessionId = Guid.NewGuid().ToString();
                controller.CurrentClient.ExtraClientData.ClientName =
                    controller.CurrentClient.ConfigClientData.ClientCert.SubjectCommonName;
                controller.CurrentClient.ExtraClientData.CustomJwtTokenIssuer = "https://identity-provider.awsqa.net";
            }
            var tokenWithSessionId = isCognito ?
                await controller.GetCognitoIdTokenWithSessionId() :
                controller.GetManualRsaTokenWithSessionId();

            Assert.NotNull(tokenWithSessionId);
            Assert.NotNull(tokenWithSessionId.Content);

            var tokenWithSessionIdJsonObj = JObject.Parse(tokenWithSessionId.Content);
            var token = tokenWithSessionIdJsonObj.GetValue("token").Value<string>();
            Assert.NotNull(token);

            var sessionId = tokenWithSessionIdJsonObj.GetValue("sessionId").Value<string>();
            var sessionIdClaimVal =
                jwtTokenHelper.GetTokenClaimValue(token, CommonConstants.Token.ClaimNames.SessionId);
            var userNameClaimName = isCognito
                ? CommonConstants.Token.ClaimNames.CognitoUserName
                : CommonConstants.Token.ClaimNames.username;
            var userNameClaimVal = jwtTokenHelper.GetTokenClaimValue(token, userNameClaimName);
            Assert.NotNull(sessionId);
            Assert.Equal(sessionId, sessionIdClaimVal);
            Assert.Equal(username, userNameClaimVal);

            var tokenValidationResultObj = controller.ValidateRsaTokenSignature(token);
            var tokenValidationResultJsonObj = JObject.Parse(tokenValidationResultObj.Content);
            var tokenValidationResult = tokenValidationResultJsonObj.GetValue("TokenValidationResult").Value<string>();
            Assert.Equal(CommonConstants.Token.ValidationResult.Success, tokenValidationResult);
        }

        /// <summary>
        /// Gets the Client for QA.
        /// </summary>
        /// <returns></returns>
        private ConfigClientData GetClient()
        {
            return new ConfigClientData
            {
                ClientCert = new ClientCert
                {
                    IssuerCommonName = "CA - QA",
                    SerialNumber = "abcd",
                    SubjectCommonName = "abc",
                },
                Cognito = new Cognito
                {
                    ClientCredentialsAuthScope = "scope",
                    ClientApp = new CognitoClient
                    {
                        ClientId = "id",
                        UserPoolId = "pool-id"
                    },
                    TokenIssuer = "https://cognito-idp.us-east-1.amazonaws.com/pool-id",
                    TokenUrl = "https://server-domain.auth.us-east-1.amazoncognito.com/oauth2/token"
                }
            };
        }

        /// <summary>
        /// Gets the Test Config Settings for QA.
        /// </summary>
        /// <returns></returns>
        private ConfigSettings GetTestConfigSettings()
        {
            var configSettings = new ConfigSettings
            {
                Clients = new[]
                {
                    GetClient()
                },
                TokenRequestTimespanSecs = 20,
                SecretsDockerFolderPath = "/run/secrets",
                CognitoSecretsFileName = "cognito-secrets.json"
            };
            return configSettings;
        }
    }
}
