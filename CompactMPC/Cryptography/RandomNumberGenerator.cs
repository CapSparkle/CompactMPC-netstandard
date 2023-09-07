using System.Numerics;

namespace CompactMPC.Cryptography
{
    public static class RandomNumberGenerator
    {
        public static byte[] GetBytes(int numberOfBytes)
        {
            byte[] bytes = new byte[numberOfBytes];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(bytes);

            return bytes;
        }

        public static BitArray GetBits(int numberOfBits)
        {
            byte[] randomBytes = GetBytes(BitArray.RequiredBytes(numberOfBits));
            return BitArray.FromBytes(randomBytes, numberOfBits);
        }

        public static BigInteger GetBigInteger(int sizeInBytes)
        {
            byte[] randomBytes = GetBytes(sizeInBytes);
            return new BigInteger(randomBytes, true);
        }
    }
}
