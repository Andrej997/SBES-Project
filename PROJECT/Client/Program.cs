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

namespace Client
{
    class Program
    {

        static void Main(string[] args)
        {
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

            #region AES ENKRIPCIJA DEKRIPCIJA TEST --- RADI KAO PODMAZANO
            /*OpenAppData o = new OpenAppData("MASINA", 112, "TNT");

            Console.WriteLine("{0}, {1}, {2}", o.ImeMasine, o.Port, o.Protokol);

            string key = SecretKey.GenerateKey();

            byte[] sifrovano = AesAlg.Encrypt(o, key);

            OpenAppData desifrovano = AesAlg.Decrypt(sifrovano, key);

            Console.WriteLine("{0}, {1}, {2}", desifrovano.ImeMasine, desifrovano.Port, desifrovano.Protokol);*/
            #endregion
        }


        private static void ChoseAppToOpen()
        {
            Console.WriteLine("Please chose one of the following apps to open:");
            Console.WriteLine("\t1.Notepad");
            Console.WriteLine("\t2.Paint");
            Console.WriteLine("Press any other key to exit");
            char key = Console.ReadKey().KeyChar;
            

            switch (key)
            {
                case '1':
                    break;
                case '2':
                    break;
                default:
                    return;
            }
        }
    }
}
