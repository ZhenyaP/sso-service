//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the FileHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

namespace IdentityProvider.SecretManager.Helpers
{
    /// <summary>
    /// Class FileHelper
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Creates file with content
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="fileContent">The file content</param>
        public static async Task CreateFileWithContentAsync(string filePath, string fileContent)
        {
            using (var writer = File.CreateText(filePath))
            {
                await writer.WriteAsync(fileContent).ConfigureAwait(false);
            }
        }
    }
}
