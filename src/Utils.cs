using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class Utils
    {

        public static string MD5Encrypt(string value)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static byte[] GetKey(string keyStr)
        {
            return Convert.FromBase64String(keyStr);
        }

        public static byte[] CreateKey(int length = 128)
        {
            byte[] key = new byte[length];
            new Random().NextBytes(key);
            return key;

        }

        public static string GetFileMD5(string fileName)
        {
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open))
                {
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    file.Close();
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Get {fileName} md5 fail {ex.Message}");
            }
        }

        public static string EncryptToSHA1(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = System.Security.Cryptography.SHA1.Create().ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString().ToLower();
        }


        public static string HmacSha1Sign(string text, string key)
        {
            HMACSHA1 myHMACSHA1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            byte[] RstRes = myHMACSHA1.ComputeHash(Encoding.UTF8.GetBytes(text));
            StringBuilder EnText = new StringBuilder();
            foreach (byte Byte in RstRes)
            {
                EnText.AppendFormat("{0:x2}", Byte);
            }
            return EnText.ToString();
        }

        public static long GetSeconds(long seconds)
        {
            DateTime dateTime = DateTime.Now.AddSeconds(seconds);
            return GetSeconds(dateTime);
        }

        public static long GetSeconds()
        {
            return GetSeconds(DateTime.Now);
        }

        public static long GetSeconds(DateTime time)
        {
            var baseDate = new System.DateTime(1970, 1, 1);
            return (long)(time - baseDate).TotalSeconds;
        }

        public static DateTime GetTime(long seconds)
        {
            var baseDate = new System.DateTime(1970, 1, 1);
            return baseDate.AddSeconds(seconds);
        }

        public static byte[] EncryptStringAES(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        public static string DecryptStringAES(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }

}

