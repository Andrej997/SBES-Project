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
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Client
{
   
    public class WCFClient : ChannelFactory<IWCFContract>, IWCFContract, IDisposable
    {
        private static IWCFContract factory;
        private static string secretKey;
        private static string machineName = System.Environment.MachineName;

        public WCFClient(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            this.Credentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            factory = this.CreateChannel();
        }

        public PovratnaVrijednost CloseApp(byte[] encrypted)
        {
            throw new NotImplementedException();
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

        public PovratnaVrijednost OpenApp(byte[] encrypted)
        {
            throw new NotImplementedException();
        }

        private void ChoseAppToOpen()
        {
            int port;
            string protokol;

            while (true)
            {

                Console.WriteLine("Please chose one of the following actions");
                Console.WriteLine("\t1.Open service");
                Console.WriteLine("\t2.Close service");
                Console.WriteLine("\t3.Check blacklist cache");
                Console.WriteLine("\t4.Exit");
                Console.WriteLine("Press any other key to exit");
                char key = Console.ReadKey().KeyChar;

                switch (key)
                {
                    case '1':
                        Console.WriteLine("Enter port number:");
                        if(Int32.TryParse(Console.ReadLine(), out port))
                        {
                            protokol = ChoseProto();
                            OpenAppData openAppData = new OpenAppData(machineName, port, protokol);
                            byte[] encrypted = AesAlg.Encrypt(openAppData.ToString(), secretKey);
                            PovratnaVrijednost pov = factory.OpenApp(encrypted);
                            if(pov == PovratnaVrijednost.USPJEH)
                            {
                                Console.WriteLine("Uspjesno ste otvorili servis!");
                            }
                            else if(pov == PovratnaVrijednost.VECOTV)
                            {
                                Console.WriteLine("Servis je vec otvoren!");
                            }
                            else if(pov == PovratnaVrijednost.NEMADOZ)
                            {
                                Console.WriteLine("Nemate dozvolu da otvorite aplikaciju!");
                            }
                            else if(pov == PovratnaVrijednost.DOS)
                            {
                                Console.WriteLine("Previse puta ste pokusali da pokrenete nedozvoljeni proces!");
                                Thread.Sleep(1000);
                                return;
                            }
                        }                        
                        break;
                    case '2':
                        Console.WriteLine("Enter port number:");
                        if (Int32.TryParse(Console.ReadLine(), out port))
                        {
                            protokol = ChoseProto();
                            OpenAppData openAppData = new OpenAppData(machineName, port, protokol);
                            PovratnaVrijednost pov =  factory.CloseApp(AesAlg.Encrypt(openAppData.ToString(), secretKey));
                            if (pov == PovratnaVrijednost.USPJEH)
                            {
                                Console.WriteLine("Uspjesno ste zatvorili servis!");
                            }
                            else if (pov == PovratnaVrijednost.NIJEOTV)
                            {
                                Console.WriteLine("Servis ne postoji!");
                            }
                            else if (pov == PovratnaVrijednost.NEMADOZ)
                            {
                                Console.WriteLine("Nemate dozvolu da zatvorite aplikaciju!");
                            }
                            else if (pov == PovratnaVrijednost.DOS)
                            {
                                Console.WriteLine("Previse puta ste pokusali da pokrenete nedozvoljeni proces!");
                                Thread.Sleep(1000);
                                return;
                            }
                        }

                        break;
                    case '4':
                        return;
                    default:
                        Console.WriteLine("Choose one of given options!");
                        break;
                }
            }

        }


        private string ChoseProto()
        {
            Console.WriteLine("Chose one of following protocols:");
            Console.WriteLine("\t1.UDP");
            Console.WriteLine("\t2.HTTP");
            Console.WriteLine("\t3.TCP");

            while(true)
            {
                char key = Console.ReadKey().KeyChar;

                switch (key)
                {
                    case '1':
                        return "UDP";
                    case '2':
                        return "HTTP";
                    case '3':
                        return "TCP";
                    default:
                        Console.WriteLine("Chose one of the given numbers");
                        break;
                }
            }
            
        }

       
    }
}
