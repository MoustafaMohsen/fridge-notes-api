using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FridgeServer.Helpers
{
    public static class EncryptionHelper
    {
        private static string EncryptionKey = "LdOA4smZ";

        public static string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            //cipherText = cipherText.Replace(" ", "+");
            //byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] cipherBytes = Encoding.UTF8.GetBytes(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }


    public static class Cryptography<E>
    where E : Encoding, new()
    {
        #region Encrypt
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Encrypt(string input, string password, string salt)
        {
            return Encrypt<AesManaged>(input, password, salt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Encrypt<T>(string input, string password, string salt)
                where T : SymmetricAlgorithm, new()
        {
            var _passwordBytes = new Rfc2898DeriveBytes(password, GetBytes(salt));

            byte[] keyBytes = _passwordBytes.GetBytes(new T().KeySize >> 3);
            byte[] vectorBytes = _passwordBytes.GetBytes(new T().BlockSize >> 3);

            return Encrypt<T>(input, keyBytes, vectorBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputBytes"></param>
        /// <param name="keyBytes"></param>
        /// <param name="vectorBytes"></param>
        /// <returns></returns>
        public static string Encrypt<T>(string input, byte[] keyBytes, byte[] vectorBytes, CipherMode cipherMode = CipherMode.ECB)
            where T : SymmetricAlgorithm, new()
        {
            using (T cipher = new T())
            {
                cipher.Mode = cipherMode;

                using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                {
                    using (var buffer = new MemoryStream())
                    {
                        using (var stream = new CryptoStream(buffer, encryptor, CryptoStreamMode.Write))
                        {
                            using (var writer = new StreamWriter(stream, new E()))
                            {
                                writer.Write(input);
                            }
                        }

                        return Convert.ToBase64String(buffer.ToArray());
                    }
                }
            }
        }
        #endregion

        #region Decrypt
        /// <summary>
        /// Protect a string with a password
        /// </summary>
        /// <param name="input">Input to be encrypted</param>
        /// <param name="password">Password to encrypt with</param>
        /// <returns>AES encrypted string</returns>
        public static string Decrypt(string input, string password, string salt)
        {
            return Decrypt<AesManaged>(input, password, salt);
        }

        /// <summary>
        /// Decrypt input using password and salt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Decrypt<T>(string input, string password, string salt)
            where T : SymmetricAlgorithm, new()
        {
            var _passwordBytes = new Rfc2898DeriveBytes(password, GetBytes(salt));
            byte[] rgbKey = _passwordBytes.GetBytes(new T().KeySize >> 3);
            byte[] rgbIV = _passwordBytes.GetBytes(new T().BlockSize >> 3);

            return Decrypt<T>(input, rgbKey, rgbIV);
        }

        public static string Decrypt<T>(string input, byte[] rgbKey, byte[] rgbIV)
            where T : SymmetricAlgorithm, new()
        {
            return Decrypt<T>(input, rgbKey: rgbKey, rgbIV: rgbIV, cipherMode: CipherMode.ECB);
        }

        /// <summary>
        /// Descrypt input using rgbKey and rgbIV
        /// </summary>
        /// <typeparam name="T">SymmetricAlgorithm</typeparam>
        /// <param name="input"></param>
        /// <param name="rgbKey"></param>
        /// <param name="rgbIV"></param>
        /// <param name="cipherMode"></param>
        /// <returns></returns>
        public static string Decrypt<T>(string input, byte[] rgbKey, byte[] rgbIV, CipherMode cipherMode)
            where T : SymmetricAlgorithm, new()
        {
            using (T cipher = new T())
            {
                cipher.Mode = cipherMode;

                using (ICryptoTransform transform = cipher.CreateDecryptor(rgbKey, rgbIV))
                {
                    using (var buffer = new MemoryStream(Convert.FromBase64String(input)))
                    {
                        using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(stream, new E()))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Get bytes using Enconding of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string str)
        {
            return new E().GetBytes(str);
        }
    }

    public static class Cryptography
    {
        private static int max_length = 32;

        /// <summary>
        /// Generate random salt
        /// </summary>
        /// <returns>Salt with a length of 32</returns>
        public static string GetSalt()
        {
            return GetSalt(max_length);
        }

        /// <summary>
        /// Generate random salt
        /// </summary>
        /// <param name="length">The length of salt</param>
        /// <returns>Salt </returns>
        public static string GetSalt(int length)
        {
            var salt = new byte[length];
            using (var cryptoService = new RNGCryptoServiceProvider())
            {
                cryptoService.GetNonZeroBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }
    }
}
