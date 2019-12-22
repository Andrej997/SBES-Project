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
using System.Xml;
using AuditContracts;

namespace Audit
{
    class Program
    {
        // lista prijavljenih klijenata
        public static List<MessageFromSM> list;
        // parametri za aktiviranje DoS, prvi je broj dozvoljenih pokusaja a drugi vremeski interval
        public static Tuple<int, int> paramsForDoS;

        static void Main(string[] args)
        {
            // Serrifikad za konektovanje
            string srvCertCN = "wcfservice";
            NetTcpBinding bindingAudit = new NetTcpBinding();
            bindingAudit.Security.Transport.ClientCredentialType 
                = TcpClientCredentialType.Certificate;
            string addressForAudit = "net.tcp://localhost:8888/RecieverAudit";
            ServiceHost hostForAudit 
                = new ServiceHost(typeof(WCFAudit));
            hostForAudit.AddServiceEndpoint(typeof(IWCFAudit), bindingAudit, addressForAudit);
            hostForAudit.Credentials.ClientCertificate.Authentication.CertificateValidationMode 
                = X509CertificateValidationMode.Custom;
            // posto je custom moramo samo da validiramo
            hostForAudit.Credentials.ClientCertificate.Authentication.CustomCertificateValidator 
                = new AuditServiceCertValidator();
            hostForAudit.Credentials.ClientCertificate.Authentication.RevocationMode 
                = X509RevocationMode.NoCheck;
            // Uzima sa masine sertifikat za konektovanje
            hostForAudit.Credentials.ServiceCertificate.Certificate 
                = AuditCertManager.GetCertificateFromStorage(
                            StoreName.My, 
                            StoreLocation.LocalMachine, 
                            srvCertCN);

            try
            {
                hostForAudit.Open();
                paramsForDoS = ReadParamsForDoS();
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
        }

        private static Tuple<int, int> ReadParamsForDoS()
        {
            Tuple<int, int> retVal = null;
            string attemps = null;
            string minutes = null;
            try
            {
                XmlReader xmlReader = XmlReader.Create("DoS.xml");
                while (xmlReader.Read())
                {
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.Name)
                        {
                            case "Attemps":
                                if (xmlReader.Read())
                                    attemps = xmlReader.Value.Trim();
                                break;
                            case "Minutes":
                                if (xmlReader.Read())
                                    minutes = xmlReader.Value.Trim();
                                break;
                        }
                    }
                }
                retVal = new Tuple<int, int>(Int32.Parse(attemps), Int32.Parse(minutes));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // ako je nekim slucajem nesto fajl ili pukao reader
                // postavicemo na 10 pokusaja u 10 minuta
                retVal = new Tuple<int, int>(10, 10);
            }

            return retVal;
        }
    }
}
