using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuditContracts;

namespace Audit
{
    class Program
    {
        static void Main(string[] args)
        {
            // Certificate connection with Audit
            string srvCertCN = "wcfservice";
            NetTcpBinding bindingAudit
                = new NetTcpBinding();
            bindingAudit.Security.Transport.ClientCredentialType
                = TcpClientCredentialType.Certificate;
            string addressForAudit = "net.tcp://localhost:12874/RecieverAudit";
            ServiceHost hostForAudit
                = new ServiceHost(typeof(WCFAudit));
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

            //hostForAudit.Credentials.ServiceCertificate.Certificate
            //    = AuditCertManager.GetCertificateFromFile(@"D:\FAX\7.SEMESTAR\SBES\PROJEKAT\PROJECT\SBES-Project\certifikati\WCFService.pfx", ss);

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
