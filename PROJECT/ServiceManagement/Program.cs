using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using Contracts;
using System.Security.Cryptography.X509Certificates;
using Manager;
using System.ServiceModel.Description;
using System.IdentityModel.Policy;
using System.Security.Cryptography;
using System.IO;
using AuditContracts;

namespace ServiceManagement
{
    class Program
    {
        public static string secretKey;
        public static ServiceHost host;
        static void Main(string[] args)
        {
            /// Define the expected service certificate. It is required to establish cmmunication using certificates.
            string srvCertCN = "wcfservice";
            secretKey = SecretKey.GenerateKey();

            NetTcpBinding bindingAudit = new NetTcpBinding();
            bindingAudit.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            
            /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
            X509Certificate2 srvCert = AuditCertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress addressForAudit = new EndpointAddress(new Uri("net.tcp://localhost:8888/RecieverAudit"),
                                      new X509CertificateEndpointIdentity(srvCert));

            using (WCFServiceAudit proxy = new WCFServiceAudit(bindingAudit, addressForAudit))
            {
                /// 1. Communication test
                Console.WriteLine("proxy " + proxy.ConnectS("TryConnect"));
                Console.WriteLine("Connection() established. Press <enter> to continue ...");                

            }

            //Windows autentifikacija
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            string address = "net.tcp://localhost:9999/Receiver";
            host = new ServiceHost(typeof(WCFService));
            host.AddServiceEndpoint(typeof(IWCFContract), binding, address);

            host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            //autorizacija
            host.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();
            host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            policies.Add(new CustomAuthorizationPolicy());
            host.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            
            try
            {
                host.Open();
                Console.WriteLine("WCFService is started.\nPress <enter> to stop ...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] {0}", e.Message);
                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
            }
            finally
            {
                host.Close();
            }
        }
        public static string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        public static void writeInTxt(string checksum)
        {
            using (StreamWriter writer = new StreamWriter("Checksum.txt"))
            {
                writer.Write(checksum);
                //writer.Write("\n");
                //writer.WriteLine();
            }
        }
        public static string readChecksum()
        {
            string newChecksum = "";
            string pom = "";

            using (StreamReader reader = new StreamReader("Checksum.txt"))
            {
                while ((pom = reader.ReadLine()) != null)
                {
                    newChecksum += pom;
                    //if(pom == null)
                    //{
                    //    newChecksum += "\n";
                    //}
                }
                //newChecksum += "\n";
            }
            return newChecksum;
        }
    }
}
