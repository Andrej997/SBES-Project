using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Manager;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Net.NetworkInformation;

namespace Client
{
   
    public class WCFClient : ChannelFactory<IWCFContract>, IWCFContract, IDisposable
    {
        private static IWCFContract factory;
        private static string secretKey;
        private static string firstMacAddress = NetworkInterface
               .GetAllNetworkInterfaces()
               .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
               .Select(nic => nic.GetPhysicalAddress().ToString())
               .FirstOrDefault();

        public WCFClient(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            this.Credentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            factory = this.CreateChannel();
        }

        public string Connect()
        {
            try
            {
                secretKey = factory.Connect();
                ChoseAppToOpen();

                return "";
            }
            catch(FaultException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("[TestCommunication] ERROR = {0}", e.Message);
            }
            return "";
        }

        public void OpenApp(byte[] encrypted)
        {
            throw new NotImplementedException();
        }

        private void ChoseAppToOpen()
        {

            while (true)
            {

                Console.WriteLine("Please chose one of the following apps to open:");
                Console.WriteLine("\t1.Notepad");
                Console.WriteLine("\t2.Paint");
                Console.WriteLine("Press any other key to exit");
                char key = Console.ReadKey().KeyChar;

                switch (key)
                {
                    case '1':
                        OpenAppData notepad = new OpenAppData(firstMacAddress, 12045, "FTP");
                        byte[] encryptedNotepad = AesAlg.Encrypt(notepad, secretKey);
                        factory.OpenApp(encryptedNotepad);
                        break;
                    case '2':
                        OpenAppData paint = new OpenAppData(firstMacAddress, 5214, "UDP");
                        byte[] encryptedPaint = AesAlg.Encrypt(paint, secretKey);
                        factory.OpenApp(encryptedPaint);
                        break;
                    default:
                        return;
                }
            }

        }
    }
}
