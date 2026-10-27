/*  Class Name      : LegacyPasswordCipher.cs
 *  Purpose         : SimpleEncrypt EncryptValue used by the portal login
 *  Author          : Dinesh C
 *  Created on      : 24-Oct-2026
 */
using System.Security.Cryptography;
using System.Text;

namespace Bosco.Auth.Infrastructure.Security
{
    public static class LegacyPasswordCipher
    {
        private const string SecretSalt = "H3#@*ALMLLlk31q4l1ncL#@";
        private static readonly byte[] InitializationVector = new byte[] { 121, 241, 10, 1, 132, 74, 11, 39, 255, 91, 45, 78, 14, 211, 22, 62 };

        public static string EncryptValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Encoding.ASCII.GetBytes(SecretSalt.PadRight(32, 'X'));
                aes.IV = InitializationVector;
                byte[] plain = Encoding.ASCII.GetBytes(value);
                byte[] cipher = aes.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);
                return Convert.ToBase64String(cipher);
            }
        }
    }
}
