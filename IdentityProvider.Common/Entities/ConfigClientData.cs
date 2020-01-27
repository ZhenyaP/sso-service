//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the Client type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityProvider.Common.Entities
{
    /// <summary>
    /// The Configuration Client Data.
    /// </summary>
    public class ConfigClientData
    {        
        /// <summary>
        /// The SSL Client Certificate.
        /// </summary>
        public ClientCert ClientCert { get; set; }

        /// <summary>
        /// The Cognito.
        /// </summary>
        public Cognito Cognito { get; set; }

        /// <summary>
        /// The Firebase.
        /// </summary>
        public Firebase Firebase { get; set; }
    }
}
