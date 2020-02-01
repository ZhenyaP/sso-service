//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the SecretsController type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using IdentityProvider.SecretManager.Helpers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdentityProvider.SecretManager.Controllers
{
    /// <summary>
    /// Class SecretsController.
    /// </summary>
    [Route("api/[controller]")]
    public class SecretsController : ControllerBase
    {
        private ISecretHelper _secretHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsController"/> class.
        /// </summary>
        /// <param name="secretHelper">The Secret Helper.</param>
        public SecretsController(ISecretHelper secretHelper)
        {
            this._secretHelper = secretHelper;
        }

        /// <summary>
        /// Uploads Certs To Docker
        /// </summary>
        [HttpPost("Upload")]
        public async Task UploadCertsToDocker()
        {
            await Task.WhenAll(this._secretHelper.CreateChainCrlFileAsync(),
                               this._secretHelper.CreateCaChainFileAsync(),
                               this._secretHelper.CreateServerCertFilesAsync(),
                               this._secretHelper.CreateThirdPartyIdentityProvidersSecretsFilesAsync())
                .ConfigureAwait(false);
        }
    }
}
