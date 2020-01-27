//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the AWSS3BucketHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace IdentityProvider.SecretManager.Helpers
{
    /// <summary>
    /// Class AWSS3BucketHelper.
    /// </summary>
    public class AWSS3BucketHelper
    {
        /// <summary>
        /// Amazon S3 Proxy client
        /// </summary>
        private readonly IAmazonS3 _amazonS3;

        /// <summary>
        /// Initializes a new instance of the <see cref="AWSS3BucketHelper"/> class.
        /// </summary>
        public AWSS3BucketHelper(IAmazonS3 amazonS3)
        {
            this._amazonS3 = amazonS3;
        }

        /// <summary>
        /// Gets the S3 Bucket object.
        /// </summary>
        /// <typeparam name="T">The S3 Bucket object type</typeparam>
        /// <param name="request">The GetObjectRequest object</param>
        /// <returns>The S3 Bucket object</returns>
        public async Task<T> GetObjectContentAsync<T>(GetObjectRequest request) where T : class
        {
            using (var response = await this._amazonS3.GetObjectAsync(request))
            {
                using (var responseStream = response.ResponseStream)
                {
                    if (typeof(T) == typeof(byte[]))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            responseStream.CopyTo(memoryStream);
                            return memoryStream.ToArray() as T;
                        }
                    }

                    if (typeof(T) == typeof(string))
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var responseBody = reader.ReadToEnd();
                            return responseBody as T;
                        }
                    }

                    return null;
                }
            }
        }
    }
}
