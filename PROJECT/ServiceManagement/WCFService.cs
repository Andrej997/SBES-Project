using Contracts;
using Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManagement
{
    public class WCFService : IWCFContract
    {
        //Dictionary<imeKorisnika, Dictionary<brojServisa, njegovEndpoint>>
        //private static Dictionary<string, Dictionary<int, string>> servisi = new Dictionary<string, Dictionary<int, string>>();
        //Dictionary<port+protokol, serviceHost>
        private static Dictionary<string, ServiceHost> servisi = new Dictionary<string, ServiceHost>();
        private static List<Restriction> blackList;

        [PrincipalPermission(SecurityAction.Demand, Role = "ExchangeSessionKey")]
        public string Connect()
        {
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User with name: {0} connected to service!", identity.Name);
           /* Console.WriteLine("IsAuthenticated {0}", identity.IsAuthenticated);
            Console.WriteLine("AuthenticationType {0}", identity.AuthenticationType);

            WindowsIdentity winIdentity = identity as WindowsIdentity;
            Console.WriteLine("Security Identifier (SID) {0}", winIdentity.User); // ovo ne moze preko IIentity id=nterfejsa jer je Windows-specific
            
            foreach (IdentityReference group in winIdentity.Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount));
                Console.WriteLine("{0}", name.ToString());
            }*/
            return Program.secretKey;           
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public PovratnaVrijednost OpenApp(byte[] encrypted)
        {
            
            OpenAppData decryted = (OpenAppData)AesAlg.Decrypt(encrypted, Program.secretKey);
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
            OpenAppData decryted = (OpenAppData)AesAlg.Decrypt(encrypted, Program.secretKey);

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
                Console.WriteLine("Doslo je do ilegalnih izmena!!! ");
                Console.WriteLine("------------------------------------------------------");
                return false;
            }
            
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "ChangeConfiguration")]
        public byte[] ReturnBlackList()
        {
            List<Restriction> list = Restriction.ReadBlackList();
            string s = Restriction.BlackListToString(list);
            return AesAlg.Encrypt(s, Program.secretKey);
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "ChangeConfiguration")]
        public bool EditBlackList(byte[] crypted)
        {
            try
            {
                List<Restriction> newBlackList = (List<Restriction>)AesAlg.Decrypt(crypted, Program.secretKey);
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
                    //if(pom == null)
                    //{
                    //    newChecksum += "\n";
                    //}
                }
                //newChecksum += "\n";
            }
            return newChecksum;
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


    }
}
