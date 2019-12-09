using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // servisni sertifitak koji ocekujemo
            string srvCertCN = "WCFSrvice";

            // sertifikat koji ocekujemo za povezivanje
            string signCertCN = String.Empty;

            string wrongCertCN = String.Empty;

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
			X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(
                                    StoreName.TrustedPeople,
                                    StoreLocation.LocalMachine, 
                                    srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/Receiver"),
                                      new X509CertificateEndpointIdentity(srvCert));

            using (WCFClient proxy = new WCFClient(binding, address))
            {
                /// 1. Communication test
                proxy.TestCommunication();
                Console.WriteLine("TestCommunication() finished. Press <enter> to continue ...");
                Console.ReadLine();

                /// 2. Digital Signing test				
                string message = "Exercise 02";

                /// Create a signature based on the "signCertCN"
                X509Certificate2 signCert = null;

                /// Create a signature using SHA1 hash algorithm
                //byte[] signature = DigitalSignature.Create();
                //proxy.SendMessage();

                Console.WriteLine("SendMessage() using {0} certificate finished. Press <enter> to continue ...", signCertCN);
                Console.ReadLine();


                /// For the same message, create a signature based on the "wrongCertCN"
                X509Certificate2 wrongSignCert = null;

                /// Create a signature using SHA1 hash algorithm
                //byte[] signature1 = DigitalSignature.Create();
                //proxy.SendMessage();

                Console.WriteLine("SendMessage() using {0} certificate finished. Press <enter> to continue ...", wrongCertCN);
                Console.ReadLine();
            }
        }
    }
}
