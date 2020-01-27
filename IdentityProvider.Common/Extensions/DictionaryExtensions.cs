//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the DictionaryExtensions type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The Dictionary Extensions.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds new Key-Value Pair to the Dictionary if value is present.
        /// </summary>
        /// <param name="dictionary">The Dictionary</param>
        /// <param name="key">The Key</param>
        /// <param name="value">The Value</param>
        public static void AddOptional(this IDictionary<string, string> dictionary, string key, string value)
        {
            if (value.IsPresent())
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Adds new Key-Value Pair to the Dictionary; if value is not present and dictionary
        /// does not allow empty values, an exception is raised.
        /// </summary>
        /// <param name="dictionary">The Dictionary</param>
        /// <param name="key">The Key</param>
        /// <param name="value">The Value</param>
        /// <param name="allowEmpty">The Value</param>
        public static void AddRequired(this IDictionary<string, string> dictionary, string key, string value, bool allowEmpty = false)
        {
            if (value.IsPresent())
            {
                dictionary.Add(key, value);
            }
            else
            {
                if (allowEmpty)
                {
                    dictionary.Add(key, "");
                }
                else
                {
                    throw new ArgumentException("Parameter is required", key);
                }
            }
        }
    }
}
