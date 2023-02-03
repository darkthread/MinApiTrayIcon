using System.Security.Cryptography;
using System.Text;

namespace MinApiTrayIcon
{
    public class AesUtil
    {

        private class AesKeyIV
        {
            public Byte[] Key = new Byte[16];
            public Byte[] IV = new Byte[16];
            public AesKeyIV(string strKey)
            {
                var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(strKey));
                Array.Copy(hash, 0, Key, 0, 16);
                Array.Copy(hash, 16, IV, 0, 16);
            }
        }
        public static string AesEncrypt(string key, string rawString)
        {
            var keyIv = new AesKeyIV(key);
            var aes = Aes.Create();
            aes.Key = keyIv.Key;
            aes.IV = keyIv.IV;
            var rawData = Encoding.UTF8.GetBytes(rawString);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                {
                    cs.Write(rawData, 0, rawData.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string AesDecrypt(string key, string encString)
        {
            var keyIv = new AesKeyIV(key);
            var aes = Aes.Create();
            aes.Key = keyIv.Key;
            aes.IV = keyIv.IV;
            var encData = Convert.FromBase64String(encString);
            byte[] buffer = new byte[8192];
            using (var ms = new MemoryStream(encData))
            {
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        using (var dec = new MemoryStream())
                        {
                            cs.CopyTo(dec);
                            return Encoding.UTF8.GetString(dec.ToArray());
                        }
                    }
                }
            }
        }
    }
}
