//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the FirebaseHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace IdentityProvider.Common.Helpers
{
    public class FirebaseHelper
    {
        private void InitializeAdminSdk(string serviceAccoundId)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.GetApplicationDefault(),
                ServiceAccountId = serviceAccoundId
            });
        }

        public async Task<string> GetCustomTokenAsync(string serviceAccoundId,
            string uid,
            Dictionary<string, object> additionalClaims = null)
        {
            InitializeAdminSdk(serviceAccoundId);

            var customToken = additionalClaims == null || additionalClaims.Count == 0 ?
                await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid) :
                await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid, additionalClaims);

            return customToken;
        }
    }
}
