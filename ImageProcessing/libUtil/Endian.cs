using System;

namespace LLE
{
    namespace Util
    {
        public class Endian
        {
	        public static bool isLittleEndian()
            {
                return BitConverter.IsLittleEndian;
            }

	        public static void swap(byte[] aDest, int aDestOffset, byte[] aSrc, int aSrcOffset, int aElements, int aElementSize, bool aSwap)
            {
                // if no swapping needed
                if (aElementSize == 1 || !aSwap)
                {
                    Buffer.BlockCopy(aSrc, aSrcOffset, aDest, aDestOffset, aElements * aElementSize);
                }
                // else swap bytes
                else
                {
                    int i;
                    for (i = 0; i < aElements; i++)
                    {
                        int j;
                        for (j = 0; j < aElementSize; j++)
                        {
                            aDest[aDestOffset + i * aElementSize + j] = aSrc[aSrcOffset + (i + 1) * aElementSize - j - 1];
                        }
                    }
                }
            }
        }
    }
}