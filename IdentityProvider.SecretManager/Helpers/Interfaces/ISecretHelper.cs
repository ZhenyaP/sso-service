using System.Threading.Tasks;

namespace IdentityProvider.SecretManager.Helpers.Interfaces
{
    public interface ISecretHelper
    {
        Task CreateChainCrlFileAsync();
        Task CreateCaChainFileAsync();
        Task CreateServerCertFilesAsync();
        Task CreateThirdPartyIdentityProvidersSecretsFilesAsync();
    }
}
