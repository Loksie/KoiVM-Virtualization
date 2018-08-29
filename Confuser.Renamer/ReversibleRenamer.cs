#region

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Confuser.Renamer
{
    public class ReversibleRenamer
    {
        private readonly RijndaelManaged cipher;
        private readonly byte[] key;

        public ReversibleRenamer(string password)
        {
            cipher = new RijndaelManaged();
            using(var sha = SHA256.Create())
            {
                cipher.Key = key = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private static string Base64Encode(byte[] buf)
        {
            return Convert.ToBase64String(buf).Trim('=').Replace('+', '$').Replace('/', '_');
        }

        private static byte[] Base64Decode(string str)
        {
            str = str.Replace('$', '+').Replace('_', '/').PadRight((str.Length + 3) & ~3, '=');
            return Convert.FromBase64String(str);
        }

        private byte[] GetIV(byte ivId)
        {
            var iv = new byte[cipher.BlockSize / 8];
            for(var i = 0; i < iv.Length; i++)
                iv[i] = (byte) (ivId ^ key[i]);
            return iv;
        }

        private byte GetIVId(string str)
        {
            var x = (byte) str[0];
            for(var i = 1; i < str.Length; i++)
                x = (byte) (x * 3 + (byte) str[i]);
            return x;
        }

        public string Encrypt(string name)
        {
            var ivId = GetIVId(name);
            cipher.IV = GetIV(ivId);
            var buf = Encoding.UTF8.GetBytes(name);

            using(var ms = new MemoryStream())
            {
                ms.WriteByte(ivId);
                using(var stream = new CryptoStream(ms, cipher.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    stream.Write(buf, 0, buf.Length);
                }

                buf = ms.ToArray();
                return Base64Encode(buf);
            }
        }

        public string Decrypt(string name)
        {
            using(var ms = new MemoryStream(Base64Decode(name)))
            {
                var ivId = (byte) ms.ReadByte();
                cipher.IV = GetIV(ivId);

                var result = new MemoryStream();
                using(var stream = new CryptoStream(ms, cipher.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    stream.CopyTo(result);
                }

                return Encoding.UTF8.GetString(result.ToArray());
            }
        }
    }
}