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
        public static byte[] Encrypt(string zaSifrovanje, string secretKey)
        {
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

        public static object Decrypt(byte[] zaDesifrovati, string secretKey)
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

            if(lines[0] == "OpenAppData")
                return new OpenAppData(lines[3], int.Parse(lines[1]), lines[2]);
            else
            {
                List<Restriction> povratnaLista = new List<Restriction>();
                int brojElemeneata = (lines.Length - (lines.Length % 3)) / 3;
                for (int i = 0; i < brojElemeneata; i++)
                {
                    Restriction res = new Restriction();
                    res.UserOrGroup = lines[i * 3];
                    res.Port = Int32.Parse(lines[i * 3 + 1]);
                    res.Protocol = lines[i * 3 + 2];
                    povratnaLista.Add(res);
                }

                return povratnaLista;
            }
        }

    }
}
