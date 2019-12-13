using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using AuditContracts;

namespace Audit
{
    class Program
    {
        static void Main(string[] args)
        {
            /// Define the expected service certificate. It is required to establish cmmunication using certificates.
            string srvCertCN = "wcfservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
            X509Certificate2 srvCert = AuditCertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9999/Receiver"),
                                      new X509CertificateEndpointIdentity(srvCert));

            using (WCFAudit proxy = new WCFAudit(binding, address))
            {
                /// 1. Communication test
                proxy.Connect("");
                Console.WriteLine("Connection() established. Press <enter> to continue ...");
                Console.ReadLine();

            }

            //#region Audit
            //ServiceSecurityAuditBehavior newAudit = new ServiceSecurityAuditBehavior();
            //newAudit.AuditLogLocation = AuditLogLocation.Application;
            //newAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;
            //newAudit.SuppressAuditFailure = true;

            //host.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            //host.Description.Behaviors.Add(newAudit);
            //#endregion
        }
    }
}
