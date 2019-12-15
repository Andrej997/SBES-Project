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
        public static byte[] Encrypt(OpenAppData zaSifrovanje1, string secretKey)
        {
            string zaSifrovanje = zaSifrovanje1.ToString();
            byte[] toBeEncrypted = null;
            byte[] encrypted = null;
            
            AesCryptoServiceProvider aesCrypto = new AesCryptoServiceProvider
            {
                Key = ASCIIEncoding.ASCII.GetBytes(secretKey),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None
            };

            toBeEncrypted = Encoding.UTF8.GetBytes(zaSifrovanje);

            aesCrypto.GenerateIV();
            ICryptoTransform aesEncrypt = aesCrypto.CreateEncryptor();

            using (MemoryStream memStream = new MemoryStream())
            {
                /* using(CryptoStream cs = new CryptoStream(memStream, aesEncrypt, CryptoStreamMode.Write))
                 {
                     using (StreamWriter sw = new StreamWriter(cs))
                         sw.Write(zaSifrovanje);
                     encrypted = memStream.ToArray();
                 }*/
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

            string s = Encoding.UTF8.GetString(decrypted);

            

            string[] lines = s.Split(',');

            return new OpenAppData(lines[2], int.Parse(lines[0]), lines[1]);
        }

    }
}
