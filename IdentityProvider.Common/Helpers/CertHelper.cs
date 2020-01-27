//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the CertHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using IdentityProvider.Common.Entities;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using ServiceStack.Text;

namespace IdentityProvider.Common.Helpers
{
    /// <summary>
    /// Class CertHelper
    /// </summary>
    public static class CertHelper
    {
        #region Private

        /// <summary>
        /// Class PasswordFinder
        /// </summary>
        private class PasswordFinder : IPasswordFinder
        {
            /// <summary>
            /// The password.
            /// </summary>
            private readonly string _password;

            /// <summary>
            /// Initializes a new instance of the <see cref="PasswordFinder"/> class.
            /// </summary>
            public PasswordFinder(string password)
            {
                this._password = password;
            }

            /// <summary>
            /// Gets the Password as a char array.
            /// </summary>
            /// <returns>The Password as a char array.</returns>
            public char[] GetPassword()
            {
                return this._password.ToCharArray();
            }
        }

        /// <summary>
        /// Gets the SSL Cert Private Key.
        /// </summary>
        /// <param name="keyParameter">The Asymmetric Key Parameter</param>
        /// <returns></returns>
        private static string GetPemEncodedKey(AsymmetricKeyParameter keyParameter)
        {
            using (TextWriter textWriter = new StringWriter())
            {
                var pemWriter = new PemWriter(textWriter);
                pemWriter.WriteObject(keyParameter);
                pemWriter.Writer.Flush();
                var key = textWriter.ToString();
                return key;
            }
        }

        private static JsonWebKeySet GetJsonWebKeySet(RsaKeyParameters keyParameters)
        {
            var e = Base64UrlEncoder.Encode(keyParameters.Exponent.ToByteArrayUnsigned());
            var n = Base64UrlEncoder.Encode(keyParameters.Modulus.ToByteArrayUnsigned());
            var dict = new Dictionary<string, string>
            {
                {"e", e},
                {"kty", "RSA"},
                {"n", n}
            };
            var hash = SHA256.Create();
            var hashBytes = hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(JsonSerializer.SerializeToString(dict)));
            var jsonWebKey = new JsonWebKey
            {
                Kid = Base64UrlEncoder.Encode(hashBytes),
                Kty = "RSA",
                E = e,
                N = n
            };
            var jsonWebKeySet = new JsonWebKeySet();
            jsonWebKeySet.Keys.Add(jsonWebKey);

            return jsonWebKeySet;
        }

        private static SigningCredentials GetSigningCredentials(AsymmetricCipherKeyPair keyPair,
            JsonWebKeySet jsonWebKeySet)
        {
            SigningCredentials signingCredentials = null;
            if (keyPair.Private is RsaPrivateCrtKeyParameters privateRsaParams)
            {
                //For more information about all these RSA private key parameters,
                // please see https://en.wikipedia.org/wiki/RSA_(cryptosystem)
                var rsa = RSA.Create(new RSAParameters
                {
                    Modulus = privateRsaParams.Modulus.ToByteArray(), // N (Modulus)
                    P = privateRsaParams.P.ToByteArray(), // P is the prime from the key generation : N = P * Q
                    Q = privateRsaParams.Q.ToByteArray(), // Q is the prime from the key generation : N = P * Q
                    DP = privateRsaParams.DP.ToByteArray(), // DP = D (mod P - 1)
                    DQ = privateRsaParams.DQ.ToByteArray(), // DQ = D (mod Q - 1)
                    InverseQ = privateRsaParams.QInv.ToByteArray(), // InverseQ = Q^(-1) (mod P)
                    D = privateRsaParams.Exponent.ToByteArray(), // D is the private exponent
                    Exponent = privateRsaParams.PublicExponent.ToByteArray() // Exponent is the public exponent
                });

                var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = jsonWebKeySet.Keys.First().Kid };
                signingCredentials = new SigningCredentials(rsaSecurityKey,
                    SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest);
            }

            return signingCredentials;
        }

        #endregion

        #region Public

        /// <summary>
        /// Gets the RSA key data.
        /// </summary>
        /// <param name="privateKey">The Private Key.</param>
        /// <param name="password">The password for the encrypted RSA Private Key
        /// (equals to null if the private key is already decrypted).</param>
        /// <returns>The RSA key data.</returns>
        public static RsaKeyData GetRsaKeyData(string privateKey, string password = null)
        {
            using (TextReader textReader = new StringReader(privateKey))
            {
                var pemReader = password == null ? new PemReader(textReader) :
                    new PemReader(textReader, new PasswordFinder(password));
                var privateKeyObject = pemReader.ReadObject();
                AsymmetricCipherKeyPair keyPair;

                JsonWebKeySet jsonWebKeySet;
                if (privateKeyObject is RsaPrivateCrtKeyParameters rsaPrivateKey)
                {
                    var rsaPublicKey = new RsaKeyParameters(false, rsaPrivateKey.Modulus, rsaPrivateKey.PublicExponent);
                    jsonWebKeySet = GetJsonWebKeySet(rsaPublicKey);
                    keyPair = new AsymmetricCipherKeyPair(rsaPublicKey, rsaPrivateKey);
                }
                else if (privateKeyObject is AsymmetricCipherKeyPair pair)
                {
                    keyPair = pair;
                    jsonWebKeySet = GetJsonWebKeySet((RsaKeyParameters)keyPair.Public);
                }
                else
                {
                    return new RsaKeyData();
                }

                var signingCredentials = GetSigningCredentials(keyPair, jsonWebKeySet);
                var rsaKeyData = new RsaKeyData
                {
                    JsonWebKeySet = jsonWebKeySet,
                    DecryptedPrivateKey = GetPemEncodedKey(keyPair.Private),
                    EncryptedPrivateKey = privateKey,
                    Password = password,
                    SigningCredentials = signingCredentials
                };
                return rsaKeyData;
            }
        }

        /// <summary>
        /// Converts DER-encoded CRL to PEM format
        /// </summary>
        /// <param name="derEncodedCrlBytes">DER-encoded CRL (in bytes)</param>
        /// <returns>PEM-encoded CRL.</returns>
        public static string ConvertDerEncodedCrlToPem(byte[] derEncodedCrlBytes)
        {
            var parser = new X509CrlParser();
            var crl = parser.ReadCrl(derEncodedCrlBytes);
            using (var textWriter = new StringWriter())
            {
                var pemWriter = new PemWriter(textWriter);
                pemWriter.WriteObject(crl);
                return textWriter.ToString();
            }
        }

        //public static string GetJsonWebKeySet(string publicKey)
        //{
        //    var certificate = new X509Certificate2();
        //    // var certificate = new X509Certificate2();
        //    var parser = new X509CertificateParser();
        //    parser.ReadCertificate(certificate.Export(X509ContentType.Cert));
        //    //parser.

        //    var publicKeyString = certificate.GetPublicKeyString();
        //    var subjectPublicKeyInfo = new SubjectPublicKeyInfo(new AlgorithmIdentifier(
        //            new DerObjectIdentifier(certificate.PublicKey.Oid.Value)),
        //        certificate.PublicKey.EncodedKeyValue.RawData);
        //    var publicKey = subjectPublicKeyInfo.GetPublicKey().ToString()PublicKeyData.GetString();
        //}

        #endregion

    }
}
