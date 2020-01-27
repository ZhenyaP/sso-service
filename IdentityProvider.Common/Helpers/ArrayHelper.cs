using System;

namespace IdentityProvider.Common.Helpers
{
    public static class ArrayHelper
    {
        public static byte[] TrimStart(byte[] array)
        {
            var firstIndex = Array.FindIndex(array, b => b != 0);
            var newSize = array.Length - firstIndex;
            var trimmedArr = new byte[newSize];
            Array.Copy(array, firstIndex, trimmedArr, 0, newSize);

            return trimmedArr;
        }
    }
}
