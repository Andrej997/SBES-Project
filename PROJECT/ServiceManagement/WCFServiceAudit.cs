using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AuditContracts;

namespace ServiceManagement
{
    public class WCFServiceAudit : ChannelFactory<IWCFAudit>, IWCFAudit, IDisposable
    {
        private static IWCFAudit factory;
        private static NetTcpBinding bind;
        private static EndpointAddress addr;

        public static IWCFAudit ReturnFactory()
        {
            if(factory == null)
            {
                new WCFServiceAudit(bind, addr);
            }

            return factory;
        }

        public WCFServiceAudit(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            /// audCertCN.SubjectName should be set to the audit's username. .NET WindowsIdentity class provides information about Windows user running the given process
            string audCertCN = "wcfservice";
            bind = binding;
            addr = address;

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
            this.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new AuditCertValidator();
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            /// Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
            this.Credentials.ClientCertificate.Certificate = AuditCertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, audCertCN);

            /*System.Security.SecureString ss = new System.Security.SecureString();
            ss.AppendChar('1');
            ss.AppendChar('2');
            ss.AppendChar('3');
            ss.AppendChar('4');*/

            //this.Credentials.ClientCertificate.Certificate
            //    = AuditCertManager.GetCertificateFromFile(@"D:\FAX\7.SEMESTAR\SBES\PROJEKAT\PROJECT\SBES-Project\certifikati\WCFService.pfx", ss);


            factory = this.CreateChannel();
        }

        public string ConnectS(string msg)
        {
            try
            {
                // Console.WriteLine("factory " + factory.ConnectS(msg));
                factory.ConnectS(msg);
                return msg;
            }
            catch (Exception e)
            {
                Console.WriteLine("[TestCommunication] ERROR = {0}", e.Message);
                return "";
            }
        }

        public void Dispose()
        {
            if (factory != null)
            {
                factory = null;
            }

            this.Close();
        }
    }
}
