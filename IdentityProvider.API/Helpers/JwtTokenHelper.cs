using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityProvider.API.Entities;
using IdentityProvider.Common;
using IdentityProvider.Common.Entities;
using IdentityProvider.Common.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using ThirdParty.BouncyCastle.Asn1;
using Newtonsoft.Json;

namespace IdentityProvider.API.Helpers
{
    public class JwtTokenHelper
    {
        #region Private

        private static object _lockRsaKeyDataObj = new object();
        private static object _lockHmacKeyDataObj = new object();

        private static RsaKeyData _rsaKeyData;
        private static string _hmacKey;
        private HttpClient _httpClient;

        /// <summary>
        /// The config settings
        /// </summary>
        private readonly ConfigSettings _configSettings;

        private string Sign(string payload, string privateKey)
        {
            var segments = new List<string>();
            var header = new { alg = "RS256", typ = "JWT" };

            var headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header));
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            segments.Add(Base64UrlEncode(headerBytes));
            segments.Add(Base64UrlEncode(payloadBytes));

            var stringToSign = string.Join(".", segments.ToArray());

            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            var keyBytes = Convert.FromBase64String(privateKey);

            var privateKeyObj = Asn1Object.FromByteArray(keyBytes);
            var privateStruct = RsaPrivateKeyStructure.GetInstance((Asn1Sequence)privateKeyObj);

            var sig = SignerUtilities.GetSigner("SHA256withRSA");

            sig.Init(true, new RsaKeyParameters(true, privateStruct.Modulus, privateStruct.PrivateExponent));

            sig.BlockUpdate(bytesToSign, 0, bytesToSign.Length);
            var signature = sig.GenerateSignature();

            segments.Add(Base64UrlEncode(signature));
            return string.Join(".", segments.ToArray());
        }

        private string UrlSafeBase64Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private string Encode(object obj)
        {
            return UrlSafeBase64Encode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));
        }

        // from JWT spec
        private string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }

        private byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 1: output += "==="; break; // Three pad chars
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }

        /// <summary>
        /// Gets the lifetime validator.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The lifetime validator.</returns>
        private LifetimeValidator GetLifetimeValidator(JwtSecurityToken token)
        {
            var validator = new LifetimeValidator(
                (notBefore, expires, securityToken, validationParameters) =>
                {
                    var validFrom = token.Payload.Iat.HasValue
                        ? TimeHelper.GetDateFromTokenTimestamp(token.Payload.Iat.Value)
                        : (token.Payload.Nbf.HasValue ?
                            TimeHelper.GetDateFromTokenTimestamp(token.Payload.Nbf.Value) :
                            DateTime.MinValue);
                    validationParameters.LifetimeValidator = null;
                    Validators.ValidateLifetime(validFrom, expires, securityToken, validationParameters);
                    return true; // if Validators.ValidateLifetime method hasn't thrown an exception, then validation passed
                });
            return validator;
        }

        #endregion

        #region Public

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenHelper"/> class.
        /// </summary>
        /// <param name="configSettings">The Config Settings.</param>
        /// <param name="httpClient">The HTTP Client.</param>
        public JwtTokenHelper(IOptions<ConfigSettings> configSettings,
                              HttpClient httpClient)
        {
            this._configSettings = configSettings.Value;
            this._httpClient = httpClient;
        }

        public RsaKeyData RsaKeyData
        {
            get
            {
                //Thread-safe setting cache data
                if (_rsaKeyData == null)
                {
                    lock (_lockRsaKeyDataObj)
                    {
                        if (_rsaKeyData == null)
                        {
                            _rsaKeyData = JsonConvert.DeserializeObject<RsaKeyData>(File.ReadAllText(Path.Combine(
                                this._configSettings.SecretsDockerFolderPath,
                                this._configSettings.RsaKeyDataFileName)));
                            //This re-initialization is used to set the SigningCredentials property in _rsaKeyData object
                            _rsaKeyData = CertHelper.GetRsaKeyData(_rsaKeyData.EncryptedPrivateKey, _rsaKeyData.Password);
                            _rsaKeyData.JsonWebKeySetSerialized = JsonConvert.SerializeObject(_rsaKeyData.JsonWebKeySet);
                        }
                    }
                }

                return _rsaKeyData;
            }
        }

        public RsaSecurityKey GetRsaSecurityKey(JwtSecurityToken jwtToken, string jwksUrl)
        {
            var tokenSigningKeysJson = _httpClient.GetStringAsync(jwksUrl).Result;
            var jsonWebKeySet = new JsonWebKeySet(tokenSigningKeysJson);
            var keyToCheck = jsonWebKeySet.Keys.First(x => x.Kid == jwtToken.Header.Kid);
            if (keyToCheck == null)
                throw new AuthenticationException($@"Could not find the token signing key with id {jwtToken.Header.Kid} from JWKs");

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(
                new RSAParameters
                {
                    Modulus = ArrayHelper.TrimStart(Base64UrlEncoder.DecodeBytes(keyToCheck.N)),
                    Exponent = Base64UrlEncoder.DecodeBytes(keyToCheck.E)
                });
            var signingKey = new RsaSecurityKey(rsa) { KeyId = keyToCheck.Kid };

            return signingKey;
        }

        /// <summary>
        /// The Base64-encoded representation of the 256 bit key,
        /// generated with a cryptographically secure pseudo random number generator (CSPRNG),
        /// used for JWT token signing/Digital Signature validation with HMAC DS algorithm.
        /// </summary>
        /// TODO: Place the HMAC Secret Key to the safe place in AWS (AWS Secrets Manager) and grab this key from AWS
        /// instead of hardcoding it here
        public string HmacKey
        {
            get
            {
                if (string.IsNullOrEmpty(_hmacKey))
                {
                    lock (_lockHmacKeyDataObj)
                    {
                        if (string.IsNullOrEmpty(_hmacKey))
                        {
                            _hmacKey = "Wf/fH3WxaGruIKSiccuYdYDC5gmzuJS4UlyEp12utoo=";
                        }
                    }
                }

                return _hmacKey;
            }
        }

        /*
        public string CreateTokenViaBouncyCastle(List<Claim> claims,
            string issuer,
            string audience,
            string privateKey)
        {
            var notBefore = DateTime.UtcNow;
            var expiresAt = DateTime.UtcNow.AddHours(1);
            var payload = Encode(new JwtPayload(issuer, audience, claims, notBefore, expiresAt));
            var token = Sign(payload, privateKey);

            return token;
        }*/

        public string CreateToken(List<Claim> claims,
            string issuer,
            string audience,
            SigningCredentials signingCredentials = null)
        {
            var notBefore = DateTime.UtcNow;
            var expiresAt = DateTime.UtcNow.AddHours(1);

            var jwtToken = new JwtSecurityToken(issuer, audience, claims, notBefore, expiresAt,
                signingCredentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return token;
        }

        public string CreateHmacToken(List<Claim> claims,
            string issuer,
            string audience,
            byte[] key,
            string keyId = null)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(key) { KeyId = keyId };
            var signingCredentials = new SigningCredentials(symmetricSecurityKey,
                SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);
            var hmacToken = CreateToken(claims, issuer, audience, signingCredentials);

            return hmacToken;
        }

        public string CreateFakeHmacToken(List<Claim> claims,
            string issuer,
            string audience,
            JsonWebKey publicRsaKey)
        {
            var key = Base64UrlEncoder.DecodeBytes(publicRsaKey.N);
            var fakeHmacToken = CreateHmacToken(claims, issuer, audience, key, publicRsaKey.KeyId);

            return fakeHmacToken;
        }

        public bool CheckIsTokenFromCognito(JwtSecurityToken jwtToken)
        {
            var isCognitoToken =
                jwtToken.Claims.FirstOrDefault(c => c.Type == CommonConstants.Token.ClaimNames.CognitoUserName) != null;

            return isCognitoToken;
        }

        public ClaimsPrincipal ValidateToken(string token,
            string validIssuer,
            string validAudience,
            SecurityKey securityKey)
        {
            var jwtToken = new JwtSecurityToken(token);
            var isCognitoToken = this.CheckIsTokenFromCognito(jwtToken);
            var parameters = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidIssuer = validIssuer,
                ValidAudience = validAudience,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateLifetime = true,
                LifetimeValidator = isCognitoToken ? this.GetLifetimeValidator(jwtToken) : null,
                ValidateAudience = true, //we should validate audience only for ID token!                
                // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
                // when validating the lifetime. As we're creating the tokens locally and validating them on the same 
                // machines which should have synchronised time, this can be set to zero. Where external tokens are
                // used, some leeway here could be useful.
                ClockSkew = TimeSpan.FromSeconds(10),
                RequireSignedTokens = true,
                RequireExpirationTime = true
            };

            var securityTokenHandler =
                new JwtSecurityTokenHandler { InboundClaimTypeMap = new Dictionary<string, string>() };
            var principal = securityTokenHandler.ValidateToken(token, parameters, out _);

            return principal;
        }

        public string GetTokenClaimValue(string token, string claimType)
        {
            var jwtToken = new JwtSecurityToken(token);
            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);

            return claim?.Value;
        }

        #endregion
    }
}
