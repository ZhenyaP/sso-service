//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the StringExtensions type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common.Extensions
{
    /// <summary>
    /// The String extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Check if string in missing.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <returns>The check result.</returns>
        public static bool IsMissing(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Check if string in present.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <returns>The check result.</returns>
        public static bool IsPresent(this string value)
        {
            return !value.IsMissing();
        }

        /// <summary>
        /// Ensure that trailing slash is present in URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The check result.</returns>
        public static string EnsureTrailingSlash(this string url)
        {
            if (!url.EndsWith("/"))
            {
                return url + "/";
            }

            return url;
        }

        /// <summary>
        /// Removes trailing slash from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The modified URL.</returns>
        public static string RemoveTrailingSlash(this string url)
        {
            if (url != null && url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }
    }
}
