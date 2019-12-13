using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
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
        
        public string Connect()
        {
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            Console.WriteLine("Name: {0}", identity.Name);
            Console.WriteLine("IsAuthenticated {0}", identity.IsAuthenticated);
            Console.WriteLine("AuthenticationType {0}", identity.AuthenticationType);

            WindowsIdentity winIdentity = identity as WindowsIdentity;
            Console.WriteLine("Security Identifier (SID) {0}", winIdentity.User); // ovo ne moze preko IIentity id=nterfejsa jer je Windows-specific
            
            foreach (IdentityReference group in winIdentity.Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount));
                Console.WriteLine("{0}", name.ToString());
            }
            return Program.secretKey;           
        }

        public bool OpenApp(byte[] encrypted)
        {
            OpenAppData decryted = AesAlg.Decrypt(encrypted, Program.secretKey);

            if(servisi.ContainsKey(string.Format("{0}{1}", decryted.Port, decryted.Protokol)))
            {
                return false;
            }

            ServiceHost host = new ServiceHost(typeof(WCFService));

            if(decryted.Protokol == "UDP")
            {
                UdpBinding binding = new UdpBinding();
                string addr = String.Format("soap.udp://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }
            else if(decryted.Protokol == "HTTP")
            {
                NetHttpBinding binding = new NetHttpBinding();
                string addr = String.Format("http://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }
            else
            {
                NetTcpBinding binding = new NetTcpBinding();
                string addr = String.Format("net.tcp://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }

            string key = String.Format("{0}{1}", decryted.Port, decryted.ImeMasine);
            servisi.Add(key, host);
            servisi[key].Open();
            return true;
        }
    }
}
