//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the AWSCognitoHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common.Helpers
{
    using Amazon.CognitoIdentityProvider;
    using Amazon.CognitoIdentityProvider.Model;
    using Amazon.Extensions.CognitoAuthentication;

    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Threading.Tasks;

    using IdentityModel.Client;

    using Entities;

    /// <summary>
    /// The AWS Cognito Helper.
    /// </summary>
    public class AWSCognitoHelper
    {
        #region Private

        /// <summary>
        /// The HTTP Client.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// The AWS Cognito Identity Provider.
        /// </summary>
        private readonly IAmazonCognitoIdentityProvider _amazonCognitoIdentityProvider;

        private async Task<string> GetIdTokenViaCustomAuthAsync(CognitoUser user,
            InitiateCustomAuthRequest initiateAuthRequest)
        {
            var authFlowResponse = await user.StartWithCustomAuthAsync(initiateAuthRequest)
                .ConfigureAwait(false);
            return authFlowResponse.AuthenticationResult.IdToken;
        }

        #endregion

        #region Public

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSCognitoHelper"/> class.
        /// </summary>
        public AWSCognitoHelper(HttpClient httpClient,
                                IAmazonCognitoIdentityProvider amazonCognitoIdentityProvider)
        {
            this._httpClient = httpClient;
            this._amazonCognitoIdentityProvider = amazonCognitoIdentityProvider;
        }

        /// <summary>
        /// Gets the client secret for Cognito App Client.
        /// </summary>
        /// <param name="cognitoClientSecretData">The Cognito Client Secret Data.</param>
        /// <returns>The Client Secret.</returns>
        public async Task<string> GetClientSecretForAppClientAsync(CognitoClient cognitoClientSecretData)
        {
            var request = new DescribeUserPoolClientRequest
            {
                ClientId = cognitoClientSecretData.ClientId,
                UserPoolId = cognitoClientSecretData.UserPoolId
            };
            var response = await this._amazonCognitoIdentityProvider.DescribeUserPoolClientAsync(request).ConfigureAwait(false);
            return response.UserPoolClient.ClientSecret;
        }

        /// <summary>
        /// Gets the access token via Client Credentials OAuth 2 flow.
        /// </summary>
        /// <param name="cognitoClient">The Cognito Client.</param>
        /// <returns>The access token.</returns>
        public async Task<string> GetClientCredentialsTokenAsync(ConfigClientData cognitoClient)
        {
            var tokenResponse = await this._httpClient.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    ClientId = cognitoClient.Cognito.ClientApp.ClientId,
                    ClientSecret = cognitoClient.Cognito.ClientApp.ClientSecret,
                    Address = cognitoClient.Cognito.TokenUrl,
                    Scope = cognitoClient.Cognito.ClientCredentialsAuthScope
                }).ConfigureAwait(false);

            return tokenResponse.AccessToken;
        }
        
        /// <summary>
        /// Gets the Identity Token via Custom Authentication in AWS Cognito.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>The Identity Token</returns>
        public async Task<string> GetCustomAuthTokenAsync(Client client)
        {
            string token;
            var userPool = new CognitoUserPool(client.ConfigClientData.Cognito.ClientApp.UserPoolId, 
                client.ConfigClientData.Cognito.ClientApp.ClientId,
                (AmazonCognitoIdentityProviderClient)this._amazonCognitoIdentityProvider,
                client.ConfigClientData.Cognito.ClientApp.ClientSecret);
            var user = new CognitoUser(client.ExtraClientData.UserName, client.ConfigClientData.Cognito.ClientApp.ClientId,
                userPool, (AmazonCognitoIdentityProviderClient)this._amazonCognitoIdentityProvider,
                client.ConfigClientData.Cognito.ClientApp.ClientSecret);

            var initiateAuthRequest = new InitiateCustomAuthRequest
            {
                AuthParameters = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    {
                        CognitoConstants.ChlgParamUsername,
                        client.ExtraClientData.UserName
                    }
                },
                ClientMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
            };
            if (!string.IsNullOrEmpty(client.ConfigClientData.Cognito.ClientApp.ClientSecret))
            {
                initiateAuthRequest.AuthParameters.Add(CognitoConstants.ChlgParamSecretHash,
                    Util.GetUserPoolSecretHash(client.ExtraClientData.UserName, client.ConfigClientData.Cognito.ClientApp.ClientId,
                        client.ConfigClientData.Cognito.ClientApp.ClientSecret));
            }

            try
            {
                token = await GetIdTokenViaCustomAuthAsync(user, initiateAuthRequest);
            }
            catch (UserNotFoundException)
            {
                await this._amazonCognitoIdentityProvider.AdminCreateUserAsync(new AdminCreateUserRequest
                {
                    MessageAction = "SUPPRESS",
                    Username = client.ExtraClientData.UserName,
                    UserPoolId = client.ConfigClientData.Cognito.ClientApp.UserPoolId
                }).ConfigureAwait(false);
                token = await GetIdTokenViaCustomAuthAsync(user, initiateAuthRequest);
            }

            return token;
        }

        /// <summary>
        /// Gets the token identifier.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The token identifier.</returns>
        public string GetTokenId(string token)
        {
            var jwtToken = new JwtSecurityToken(token);
            return jwtToken.Id;
        }

        #endregion
    }
}
