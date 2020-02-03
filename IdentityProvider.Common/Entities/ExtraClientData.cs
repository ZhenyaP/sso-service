using System.Collections.Generic;
using System.Security.Claims;
using IdentityProvider.Common.Helpers;
using Org.BouncyCastle.X509;

namespace IdentityProvider.Common.Entities
{
    public class ExtraClientData
    {
        public X509Certificate ClientCert { get; set; }

        /// <summary>
        /// Gets or sets the Client Name.
        /// </summary>
        /// <value>The User Name.</value>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the User Name.
        /// </summary>
        /// <value>The User Name.</value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Session ID.
        /// </summary>
        /// <value>The Session ID.</value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the Client IP address.
        /// </summary>
        /// <value>The Client IP address.</value>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the original URL scheme (HTTP/HTTPS).
        /// </summary>
        /// <value>The original URL scheme (HTTP/HTTPS).</value>
        public string ForwardedProto { get; set; }

        /// <summary>
        /// Gets or sets the Custom JWT Token Issuer.
        /// </summary>
        /// <value>The Custom JWT Token Issuer.</value>
        public string CustomJwtTokenIssuer { get; set; }

        /// <summary>
        /// Gets or sets the User Claims.
        /// </summary>
        /// <value>The User Claims.</value>
        public List<Claim> Claims => new List<Claim>
        {
            new Claim(CommonConstants.Token.ClaimNames.username, this.UserName),
            new Claim(CommonConstants.Token.ClaimNames.SessionId, this.SessionId),
            new Claim(CommonConstants.Token.ClaimNames.ClientIp, this.IpAddress),
            new Claim(CommonConstants.Token.ClaimNames.Cnf, $"{{\"{CommonConstants.Token.ClaimNames.X5tSha256}\": \"{this.ClientCert.GetSha256Thumbprint()}\"}}")
        };
    }
}
