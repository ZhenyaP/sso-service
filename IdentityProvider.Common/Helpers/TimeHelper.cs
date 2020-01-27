//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the TimeHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;

namespace IdentityProvider.Common.Helpers
{
    using System.Diagnostics;

    /// <summary>
    /// Class TimeHelper
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        /// Gets the elapsed milliseconds.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        /// <returns>The elapsed milliseconds.</returns>
        public static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

        /// <summary>
        /// Gets the date from token timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>The date.</returns>
        public static DateTime GetDateFromTokenTimestamp(int timestamp)
        {
            var secondsAfterBaseTime =
                Convert.ToInt64(Math.Truncate(Convert.ToDouble(timestamp, CultureInfo.InvariantCulture)));
            return EpochTime.DateTime(secondsAfterBaseTime);
        }
    }
}
