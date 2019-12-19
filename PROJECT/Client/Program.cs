using Contracts;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO.Ports;

namespace Client
{
    class Program
    {
        public static string secretKey;

        static void Main(string[] args)
        {
            Console.ReadLine();
            //povezivanje na server koristeci windows autentifikaciju
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/Receiver";
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            
            
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(address));

            using (WCFClient proxy = new WCFClient(binding, endpointAddress))
            {
               proxy.Connect("");
            }
            Console.ReadLine();
            #region AES ENKRIPCIJA DEKRIPCIJA TEST --- RADI KAO PODMAZANO
            /*OpenAppData o = new OpenAppData("MASINA", 112, "TNT");

            Console.WriteLine("{0}, {1}, {2}", o.ImeMasine, o.Port, o.Protokol);

            string key = SecretKey.GenerateKey();

            byte[] sifrovano = AesAlg.Encrypt(o, key);

            OpenAppData desifrovano = AesAlg.Decrypt(sifrovano, key);

            Console.WriteLine("{0}, {1}, {2}", desifrovano.ImeMasine, desifrovano.Port, desifrovano.Protokol);*/
            #endregion
        }


        
    }
}
