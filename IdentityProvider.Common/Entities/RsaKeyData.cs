using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace IdentityProvider.Common.Entities
{
    [JsonObject]
    public class RsaKeyData
    {
        public JsonWebKeySet JsonWebKeySet { get; set; }

        [JsonIgnore]
        public string JsonWebKeySetSerialized { get; set; }

        public string EncryptedPrivateKey { get; set; }

        public string Password { get; set; }

        public string DecryptedPrivateKey { get; set; }

        [JsonIgnore]
        public SigningCredentials SigningCredentials { get; set; }
    }
}
