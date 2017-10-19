using System;
using System.IO;
using System.Security.Cryptography;

namespace Weber
{
    public static class Utilities
    {
        public static void SetMemory(byte[] data, byte value, int size)
        {
            for (int i = 0; i < size; i++)
            {
                data[i] = value;
            }
        }

        public static void CopyMemory(byte[] dest, byte[] source, int size)
        {
            CopyMemory(dest, source, size, /*destOffset=*/0, /*destOffset=*/0);
        }

        public static void CopyMemory(byte[] dest, byte[] source, int size, int destOffset, int sourceOffset)
        {
            for (int i = 0; i < size; i++)
            {
                dest[i + destOffset] = source[i + sourceOffset];
            }
        }

        public static bool CompareMemory(byte[] buffer1, byte[] buffer2, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (buffer1[i] != buffer2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static byte[] Decrypt(byte[] key, byte[] challenge)
        {
            try
            {
                Aes aes = Aes.Create();
                aes.Padding = PaddingMode.None;
                aes.Mode = CipherMode.ECB;
                aes.Key = key;
                ICryptoTransform decrypter = aes.CreateDecryptor();
                byte[] decrypted = decrypter.TransformFinalBlock(challenge, 0, challenge.Length);

                return decrypted;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static byte[] Encrypt(byte[] key, byte[] clearText)
        {
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Key = key;
            aes.Padding = PaddingMode.Zeros;
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream_Encryptor = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream_Encryptor.Write(clearText, 0, clearText.Length);

            byte[] encryptedString = memoryStream.ToArray();
            return encryptedString;
        }
    }
}
