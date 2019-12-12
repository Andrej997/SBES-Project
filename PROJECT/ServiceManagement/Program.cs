using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using Contracts;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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
        static void Main(string[] args)
        {
            // Certificate connection with Audit
            string srvCertCN = AuditFormatter.ParseName(WindowsIdentity.GetCurrent().Name);
            NetTcpBinding bindingAudit 
                = new NetTcpBinding();
            bindingAudit.Security.Transport.ClientCredentialType
                = TcpClientCredentialType.Certificate;
            string addressForAudit = "net.tcp://localhost:9999/Receiver";
            ServiceHost hostForAudit 
                = new ServiceHost(typeof(WCFServiceAudit));
            hostForAudit.AddServiceEndpoint(typeof(IWCFAudit), bindingAudit, addressForAudit);
            hostForAudit.Credentials.ClientCertificate.Authentication.CertificateValidationMode 
                = X509CertificateValidationMode.PeerTrust;
            hostForAudit.Credentials.ClientCertificate.Authentication.CustomCertificateValidator 
                = new AuditServiceCertValidator();
            hostForAudit.Credentials.ClientCertificate.Authentication.RevocationMode 
                = X509RevocationMode.NoCheck;
            hostForAudit.Credentials.ServiceCertificate.Certificate
                = AuditCertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            System.Security.SecureString ss = new System.Security.SecureString();
            ss.AppendChar('1');
            ss.AppendChar('2');
            ss.AppendChar('3');
            ss.AppendChar('4');

            hostForAudit.Credentials.ServiceCertificate.Certificate
                = AuditCertManager.GetCertificateFromFile(@"D:\FAX\7.SEMESTAR\SBES\PROJEKAT\PROJECT\SBES-Project\certifikati\WCFService.pfx", ss);

            try
            {
                hostForAudit.Open();
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
                hostForAudit.Close();
            }



            secretKey = SecretKey.GenerateKey();
            //Process notePad = new Process();
            //notePad.StartInfo.FileName = "mspaint.exe";
            ////notePad.StartInfo.Arguments = "mytextfile.txt";
            //notePad.Start();
            
            //*****

            Blacklist blacklist = new Blacklist();

            string checksum = checkMD5("Blacklist.xml");

            byte[] ba = Encoding.Default.GetBytes(checksum); //ako hocemo hex zapis
            string hexString = BitConverter.ToString(ba);   //primer: 9B-B0-0A-05-D2-12-D0-FD-EE-85-36-86-0C-15-43-99

            writeInTxt(hexString);

            string newChecksum = readChecksum(); //treba ubaciti gde god proveravamo integritet blacklist-e

            if (hexString == newChecksum)
            {
                Console.WriteLine("Nije bilo izmena . . . ");
            }
            else
            {
                Console.WriteLine("Doslo je do izmena !!! ");
            }


            //*****

            //Windows autentifikacija
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            string address = "net.tcp://localhost:9999/Receiver";
            ServiceHost host = new ServiceHost(typeof(WCFService));
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
