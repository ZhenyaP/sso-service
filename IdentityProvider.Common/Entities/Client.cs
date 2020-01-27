namespace IdentityProvider.Common.Entities
{
    public class Client
    {
        public ConfigClientData ConfigClientData { get; set; }

        public ExtraClientData ExtraClientData { get; set; }

        public string CognitoJwksUrl => $"{ConfigClientData.Cognito?.TokenIssuer}/.well-known/jwks.json";
    }
}
