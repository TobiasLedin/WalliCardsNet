using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text;

namespace WalliCardsNet.API.Helpers
{
    public static class EncryptionHelper
    {
        // Encryption key needs to be a 32 byte key (256-bit)!
        private static readonly string _key = Environment.GetEnvironmentVariable("ENCRYPTION-KEY")
            ?? throw new NullReferenceException("Encryption key missing");

        public static async Task<string> EncryptAsync(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_key);
                aes.Mode = CipherMode.ECB;

                using (var encryptor = aes.CreateEncryptor(aes.Key, null))
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        var plainBytes = Encoding.UTF8.GetBytes(plainText);

                        await cryptoStream.WriteAsync(plainBytes, 0, plainBytes.Length);
                        await cryptoStream.FlushFinalBlockAsync();

                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        public static async Task<string> DecryptAsync(string encryptedText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_key);
                aes.Mode = CipherMode.ECB;

                using (var decryptor = aes.CreateDecryptor(aes.Key, null))
                using (var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        var reader = new StreamReader(cryptoStream);

                        return await reader.ReadToEndAsync();
                    }
                }
            }
        }
    }
}
