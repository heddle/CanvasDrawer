using System;

namespace CanvasDrawer.Util
{
    public static class Bits
    {

        /// <summary>
        /// Check if a given bit is set.
        /// </summary>
        /// <param name="bits">The word being used to store flags bitwise.</param>
        /// <param name="b">The bit to check.</param>
        /// <returns>true if the given bit is set.</returns>
        public static bool CheckBit(int bits, int b)
        {
            return ((bits & b) == b);
        }

        /// <summary>
        /// Set a given bit pattern. Bits not in the pattern are unchanged.
        /// </summary>
        /// <param name="bits">The word being used to store flags bitwise.</param>
        /// <param name="b">The bit pattern to set..</param>
        /// <returns>The word being modified is returned.</returns>
        public static int SetBit(int bits, int b)
        {
            bits |= b;
            return bits;
        }

        /// <summary>
        /// Set a given bit at a location. Other bits are unchanged.
        /// </summary>
        /// <param name="bits">The word being used to store flags bitwise.</param>
        /// <param name="b">The bit location to set..</param>
        /// <returns>The word being modified is returned.</returns>
        public static int SetBitAtLocation(int bits, int bitIndex)
        {
            int b = 1 << bitIndex;
            bits |= b;
            return bits;
        }

        /// <summary>
        /// Clear a given bit pattern. Bits not in the pattern are unchanged.
        /// </summary>
        /// <param name="bits">The word being used to store flags bitwise.</param>
        /// <param name="b">The bit pattern to clear..</param>
        /// <returns>The word being modified is returned.</returns>
        public static int ClearBit(int bits, int b)
        {
            bits &= (~b);
            return bits;
        }

        /// <summary>
        /// Toggle a given bit pattern. Bits not in the pattern are unchanged.
        /// </summary>
        /// <param name="bits">The word being used to store flags bitwise.</param>
        /// <param name="b">The bit pattern to toggle..</param>
        /// <returns>The word being modified is returned.</returns>
        public static int ToggleBit(int bits, int b)
        {
            bits ^= b;
            return bits;
        }

        /// <summary>
        /// Get the location of the first set bit.
        /// </summary>
        /// <param name="bits">The word to check.</param>
        /// <returns>The location of the first bit set.</returns>
        public static int FirstBit(int bits)
        {

            for (int b = 0; bits != 0; bits = bits >> 1)
            {
                if ((bits & 01) == 01)
                {
                    return b;
                }
                b++;
            }

            return -1;
        }

        /// <summary>
        /// Count the number of bits set in a word.
        /// </summary>
        /// <param name="bits">The word whose bits are counted.</param>
        /// <returns>The number of bits set in the word.</returns>
        public static int CountBits(int bits)
        {
            int count;

            for (count = 0; bits != 0; bits = bits >> 1)
            {
                if ((bits & 01) == 01)
                {
                    count++;
                }
            }
            return count;
        }

    }
}
