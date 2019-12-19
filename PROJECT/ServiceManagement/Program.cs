﻿using System;
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
        static void Main(string[] args)
        {
            /// Define the expected service certificate. It is required to establish cmmunication using certificates.
            string srvCertCN = "wcfservice";
            secretKey = SecretKey.GenerateKey();
            /*string checksum = checkMD5("BlackList.xml");
            byte[] ba = Encoding.Default.GetBytes(checksum); //ako hocemo hex zapis
            string hexString = BitConverter.ToString(ba);   //primer: 9B-B0-0A-05-D2-12-D0-FD-EE-85-36-86-0C-15-43-99
            writeInTxt(hexString);*/


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

            /*Restriction res = new Restriction();
            res.UserOrGroup = "Jadnici";
            res.Port = 5214;
            res.Protocol = "UDP";

            Restriction res1 = new Restriction();
            res1.UserOrGroup = "Jadnik";
            res1.Port = 10000;
            res1.Protocol = "HTTP";

            Restriction res2 = new Restriction();
            res2.UserOrGroup = "Genijalci";
            res2.Port = 8624;
            res2.Protocol = "TCP";

            Restriction res3 = new Restriction();
            res3.UserOrGroup = "Umetnici";
            res3.Port = 13200;
            res3.Protocol = "HTTP";

            List<Restriction> ress = new List<Restriction>() { res, res1, res2, res3 };

            Restriction.WriteBlackList(ress);*/

            
            //Process notePad = new Process();
            //notePad.StartInfo.FileName = "mspaint.exe";
            ////notePad.StartInfo.Arguments = "mytextfile.txt";
            //notePad.Start();
            
            //*****

            /*Blacklist blacklist = new Blacklist();
            blacklist.WriteInXml();

            string checksum = checkMD5("Blacklist.xml");

            byte[] ba = Encoding.Default.GetBytes(checksum); //ako hocemo hex zapis
            string hexString = BitConverter.ToString(ba);   //primer: 9B-B0-0A-05-D2-12-D0-FD-EE-85-36-86-0C-15-43-99

            writeInTxt(hexString);

            string hexValidChecksum = readChecksum(); //citamo checksum koja je validna

            //provera validnosti
            string newChecksum = checkMD5("Blacklist.xml");
            byte[] newC = Encoding.Default.GetBytes(newChecksum); 
            string hexNewChecksum = BitConverter.ToString(newC);
            

            if (hexNewChecksum == hexValidChecksum)
            {
                Console.WriteLine("Nije bilo izmena . . . ");
            }
            else
            {
                Console.WriteLine("Doslo je do izmena !!! ");
            }
            //****

            List<Restriction> restrictionList = new List<Restriction>();

            restrictionList = blacklist.ReadFromXml();

            blacklist.WriteInXml(restrictionList);

            //******/

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
