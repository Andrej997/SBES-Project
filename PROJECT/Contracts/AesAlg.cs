using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class AesAlg
    {
        public static byte[] Encrypt(OpenAppData zaSifrovanje, string secretKey)
        {
            byte[] toBeEncrypted = null;
            byte[] encrypted = null;
            
            AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider
            {
                Key = ASCIIEncoding.ASCII.GetBytes(secretKey),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None
            };

            aesCrypto.GenerateIV();
            ICryptoTransform aesEncrypt = aesCrypto.CreateEncryptor();
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream memStream = new MemoryStream())
            {
                bf.Serialize(memStream, zaSifrovanje);
                toBeEncrypted = memStream.ToArray();
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                CryptoStream cryptoStream = new CryptoStream(memStream, aesEncrypt, CryptoStreamMode.Write);
                
                cryptoStream.Write(toBeEncrypted, 0, toBeEncrypted.Length);
                encrypted = aesCrypto.IV.Concat(memStream.ToArray()).ToArray();
                
            }

            return encrypted;
        }

        public static OpenAppData Decrypt(byte[] zaDesifrovati, string secretKey)
        {
            byte[] decrypted = null;

            AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider
            {
                Key = ASCIIEncoding.ASCII.GetBytes(secretKey),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None
            };

            aesCrypto.IV = zaDesifrovati.Take(aesCrypto.BlockSize / 8).ToArray();			
            ICryptoTransform aesDecrypt = aesCrypto.CreateDecryptor();

            using (MemoryStream mStream = new MemoryStream(zaDesifrovati.Skip(aesCrypto.BlockSize / 8).ToArray()))
            {
                using (CryptoStream cryptoStream = new CryptoStream(mStream, aesDecrypt, CryptoStreamMode.Read))
                {
                    decrypted = new byte[zaDesifrovati.Length - aesCrypto.BlockSize / 8];    
                    cryptoStream.Read(decrypted, 0, decrypted.Length);
                }
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(decrypted))
            {
                object obj = bf.Deserialize(ms);
                return (OpenAppData)obj;
            }
        }
    }
}
