using Contracts;
using Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace ServiceManagement
{
    public class WCFService : IWCFContract
    {
        private static Dictionary<string, ServiceHost> servisi = new Dictionary<string, ServiceHost>();
        private static List<Restriction> blackList;
        //Dictionary<korisnik, kljucSesije>
        private static Dictionary<string, string> sessionKeys = new Dictionary<string, string>();

        [PrincipalPermission(SecurityAction.Demand, Role = "ExchangeSessionKey")]
        public bool Connect(string sessionKey)
        {
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User with name: {0} connected to service!", identity.Name);
            if (sessionKeys.ContainsKey(Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name)))
                sessionKeys[Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name)] = sessionKey;
            else
                sessionKeys.Add(Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name), sessionKey);
            return true;           
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public PovratnaVrijednost OpenApp(byte[] encrypted)
        {
            
            OpenAppData decryted = (OpenAppData)AesAlg.Decrypt(encrypted, sessionKeys[Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name)]);
            Console.WriteLine("------------------ OTVARANJE SERVISA ------------------");
            Console.WriteLine("Korisnik {0} je zatrazio otvaranje novog servisa na portu {1} sa protokolom {2}", Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name)
                , decryted.Port, decryted.Protokol);
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity winIdentity = identity as WindowsIdentity;

            string user = Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
            List<string> groups = GetUsergroups(winIdentity.Groups);


            blackList = Restriction.ReadBlackList();

            if(Restriction.IsRestricted(blackList, decryted, user, groups))
            {
                Console.WriteLine("Korisnik nema dozvolu za otvaranje servisa na datom portu ili sa datim protokolom.");
                string pov = WCFServiceAudit.ReturnFactory().ConnectS(string.Format("{0}|{1}|{2}", user, decryted.Protokol, decryted.Port));
                Console.WriteLine("------------------ OTVARANJE NEUSPESNO ------------------");
                if (pov == "DoS")
                    return PovratnaVrijednost.DOS;
                return PovratnaVrijednost.NEMADOZ;;
                
            }


            if (servisi.ContainsKey(string.Format("{0}", decryted.Port)))
            {
                Console.WriteLine("Servis je vec otvoren na datom portu");
                Console.WriteLine("------------------ OTVARANJE NEUSPESNO ------------------");
                return PovratnaVrijednost.VECOTV;
            }

            ServiceHost host = new ServiceHost(typeof(WCFService));

            if(decryted.Protokol == "UDP")
            {
                Console.WriteLine("Otvaranje UDP konekcije");
                UdpBinding binding = new UdpBinding();
                string addr = String.Format("soap.udp://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }
            else if(decryted.Protokol == "HTTP")
            {
                Console.WriteLine("Otvaranje HTTP konekcije");
                NetHttpBinding binding = new NetHttpBinding();
                string addr = String.Format("http://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }
            else
            {
                Console.WriteLine("Otvaranje TCP konekcije");
                NetTcpBinding binding = new NetTcpBinding();
                string addr = String.Format("net.tcp://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }

            string key = String.Format("{0}", decryted.Port);
            servisi.Add(key, host);
            servisi[key].Open();
            Console.WriteLine("------------------ OTVARANJE USPESNO ------------------");
            return PovratnaVrijednost.USPJEH;
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public PovratnaVrijednost CloseApp(byte[] encrypted)
        {
            OpenAppData decryted = (OpenAppData)AesAlg.Decrypt(encrypted, sessionKeys[Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name)]);

            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity winIdentity = identity as WindowsIdentity;

            string user = Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name);
            List<string> groups = GetUsergroups(winIdentity.Groups);


            blackList = Restriction.ReadBlackList();

            if (Restriction.IsRestricted(blackList, decryted, user, groups))
            {

                string pov = WCFServiceAudit.ReturnFactory().ConnectS(string.Format("{0}|{1}|{2}", user, decryted.Protokol, decryted.Port));
                if (pov == "DOS")
                    return PovratnaVrijednost.DOS;
                return PovratnaVrijednost.NEMADOZ; ;
            }

            string key = string.Format("{0}", decryted.Port);
            if (servisi.ContainsKey(key))
            {
                servisi[key].Close();
                return PovratnaVrijednost.USPJEH;
            }

            
            return PovratnaVrijednost.NIJEOTV;
        }



        [PrincipalPermission(SecurityAction.Demand, Role = "ChangeConfiguration")]
        public bool IsBlackListValid()
        {
            Console.WriteLine("------------------ PROVERA CRNE LISTE ------------------");
            Console.WriteLine("Korisnik {0} je zatrazio proveru konfiguracije crne liste", Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name));
            string hexValidChecksum = readChecksum();
            string newChecksum = checkMD5("Blacklist.xml");
            byte[] newC = Encoding.Default.GetBytes(newChecksum);
            string hexNewChecksum = BitConverter.ToString(newC);

            if (hexNewChecksum == hexValidChecksum)
            {
                Console.WriteLine("Nije bilo izmena crne liste!");
                Console.WriteLine("------------------------------------------------------");
                return true;
            }
            else
            {
                string pov = WCFServiceAudit.ReturnFactory().ConnectS("Blacklist");
                Console.WriteLine("Doslo je do ilegalnih izmena!!! ");
                Console.WriteLine("------------------------------------------------------");
                Program.host.Close();
                return false;
            }
            
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "ChangeConfiguration")]
        public byte[] ReturnBlackList()
        {
            List<Restriction> list = Restriction.ReadBlackList();
            string s = Restriction.BlackListToString(list);
            return AesAlg.Encrypt(s, sessionKeys[Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name)]);
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "ChangeConfiguration")]
        public bool EditBlackList(byte[] crypted)
        {
            try
            {
                List<Restriction> newBlackList = (List<Restriction>)AesAlg.Decrypt(crypted, sessionKeys[Formatter.ParseName(Thread.CurrentPrincipal.Identity.Name)]);
                Restriction.WriteBlackList(newBlackList);

                string checksum = checkMD5("Blacklist.xml");
                byte[] ba = Encoding.Default.GetBytes(checksum);
                string hexString = BitConverter.ToString(ba);
                writeInTxt(hexString);
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return false;
            }           

            
        }

        private List<string> GetUsergroups(IdentityReferenceCollection irc)
        {
            List<string> lista = new List<string>();

            foreach (IdentityReference group in irc)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount));
                lista.Add(name.ToString());
            }

            return lista;
        }

        private string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        private string readChecksum()
        {
            string newChecksum = "";
            string pom = "";

            using (StreamReader reader = new StreamReader("Checksum.txt"))
            {
                while ((pom = reader.ReadLine()) != null)
                {
                    newChecksum += pom;
                }
            }
            return newChecksum;
        }

        public static void writeInTxt(string checksum)
        {
            using (StreamWriter writer = new StreamWriter("Checksum.txt"))
            {
                writer.Write(checksum);
            }
        }


    }
}
