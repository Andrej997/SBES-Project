using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Contracts;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Diagnostics;
using Manager;
using System.ServiceModel.Description;
using System.IdentityModel.Policy;
using System.Security.Cryptography;
using System.IO;

namespace ServiceManagement
{
    class Program
    {
        public static string secretKey;
        static void Main(string[] args)
        {
            secretKey = SecretKey.GenerateKey();
            //Process notePad = new Process();
            //notePad.StartInfo.FileName = "mspaint.exe";
            ////notePad.StartInfo.Arguments = "mytextfile.txt";
            //notePad.Start();

            /// srvCertCN.SubjectName should be set to the service's username. .NET WindowsIdentity class provides information about Windows user running the given process
			//string srvCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            //*****

            CreateBlacklist blacklist = new CreateBlacklist();

            string checksum = checkMD5("Blacklist.xml");

            byte[] ba = Encoding.Default.GetBytes(checksum); //ako hocemo hex zapis
            string hexString = BitConverter.ToString(ba);   //primer: 9B-B0-0A-05-D2-12-D0-FD-EE-85-36-86-0C-15-43-99

            writeInTxt(hexString);

            string newChecksum = readChecksum(); //treba ubaciti gde god proveravamo integritet blacklist-e

            if(hexString == newChecksum)
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

            using(StreamReader reader = new StreamReader("Checksum.txt"))
            {
                while((pom = reader.ReadLine()) != null)
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
