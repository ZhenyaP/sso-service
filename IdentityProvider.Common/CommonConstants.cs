//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the CommonConstants type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common
{
    /// <summary>
    /// Class CommonConstants
    /// </summary>
    public static class CommonConstants
    {
        /// <summary>
        /// The AWS Keys File Name
        /// </summary>
        public const string AwsKeysFileName = "/app/aws-keys.txt";

        /// <summary>
        /// The HttpHeaders class
        /// </summary>
        public static class HttpHeaders
        {
            /// <summary>
            /// The SSLClientSDN constant
            /// </summary>
            public const string SSLClientSDN = "X-SSL-CLIENT-S-DN";

            /// <summary>
            /// The SSLClientIDN constant
            /// </summary>
            public const string SSLClientIDN = "X-SSL-CLIENT-I-DN";

            /// <summary>
            /// The SSLClientSerial constant
            /// </summary>
            public const string SSLClientSerial = "X-SSL-CLIENT-SERIAL";

            /// <summary>
            /// The SSLClientSerial constant
            /// </summary>
            public const string SSLClientCert = "X-SSL-CLIENT-CERT";

            /// <summary>
            /// The ForwardedFor constant
            /// </summary>
            public const string ForwardedFor = "X-Forwarded-For";

            /// <summary>
            /// The ForwardedProto constant
            /// </summary>
            public const string ForwardedProto = "X-Forwarded-Proto";
        }

        public static class Token
        {
            public static class ValidationResult
            {
                public const string Success = "success";
                public const string Failure = "failure";
            }

            public static class ClaimNames
            {
                public const string username = "username";
                public const string SessionId = "SessionId";
                public const string ClientIp = "clientIP";
                public const string CognitoUserName = "cognito:username";
                public const string Cnf = "cnf";
                public const string X5tSha256 = "x5t#S256";
            }
        }

        public static class Cert
        {
            public const string RsaPrivateKeyHeader = "-----BEGIN RSA PRIVATE KEY-----";
            public const string RsaPrivateKeyFooter = "-----END RSA PRIVATE KEY-----";

            public const string ClientCertSubjectCommonName = "ABC";
        }
    }
}
