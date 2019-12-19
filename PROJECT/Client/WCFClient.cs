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
        private static string secretKey = SecretKey.GenerateKey();
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

        public bool Connect(string s)
        {
            try
            {
                if (factory.Connect(secretKey))
                    ChoseAppToOpen();
                else
                    return false;

                return true;
            }
            catch(FaultException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("[TestCommunication] ERROR = {0}", e.Message);
            }
            return false;
        }

        public bool EditBlackList(byte[] crypted)
        {
            return factory.EditBlackList(crypted);
        }

        public bool IsBlackListValid()
        {
            return factory.IsBlackListValid();
        }

        public PovratnaVrijednost OpenApp(byte[] encrypted)
        {
            throw new NotImplementedException();
        }

        public byte[] ReturnBlackList()
        {
            return factory.ReturnBlackList();
        }

        private void ChoseAppToOpen()
        {
            int port = 0;
            string protokol;

            while (true)
            {

                Console.WriteLine("Please chose one of the following actions");
                Console.WriteLine("\t1.Open service");
                Console.WriteLine("\t2.Close service");
                Console.WriteLine("\t3.Check blacklist cache");
                Console.WriteLine("\t4.Edit blacklist");
                Console.WriteLine("\t5.Exit");
                Console.WriteLine("Press any other key to exit");
                char key = Console.ReadKey().KeyChar;

                switch (key)
                {
                    case '1':
                        try
                        {
                            Console.WriteLine("Enter port number:");
                            if (Int32.TryParse(Console.ReadLine(), out port))
                            {
                                protokol = ChoseProto();
                                OpenAppData openAppData = new OpenAppData(machineName, port, protokol);
                                byte[] encrypted = AesAlg.Encrypt(openAppData.ToString(), secretKey);
                                PovratnaVrijednost pov = factory.OpenApp(encrypted);
                                if (pov == PovratnaVrijednost.USPJEH)
                                {
                                    Console.WriteLine("Uspjesno ste otvorili servis!");
                                }
                                else if (pov == PovratnaVrijednost.VECOTV)
                                {
                                    Console.WriteLine("Servis je vec otvoren!");
                                }
                                else if (pov == PovratnaVrijednost.NEMADOZ)
                                {
                                    Console.WriteLine("Nemate dozvolu da otvorite aplikaciju!");
                                }
                                else if (pov == PovratnaVrijednost.DOS)
                                {
                                    Console.WriteLine("Previse puta ste pokusali da pokrenete nedozvoljeni proces!");
                                    Thread.Sleep(1000);
                                    return;
                                }
                            }
                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Request failed! Error message: " + e.Message);
                        }
                        break;
                    case '2':
                        try
                        {
                            Console.WriteLine("Enter port number:");
                            if (Int32.TryParse(Console.ReadLine(), out port))
                            {
                                protokol = ChoseProto();
                                OpenAppData openAppData = new OpenAppData(machineName, port, protokol);
                                PovratnaVrijednost pov = factory.CloseApp(AesAlg.Encrypt(openAppData.ToString(), secretKey));
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
                                                      
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Request failed! Error message: " + e.Message);
                        }
                        
                        break;
                    case '3':
                        Console.WriteLine("Checking blacklist cache....");
                        try
                        {
                            if (factory.IsBlackListValid())
                            {
                                Console.WriteLine("Black list is valid!");
                                
                            }
                            else
                            {
                                Console.WriteLine("Black list is not valid!");
                            }
                            
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Request failed! Error message: " + e.Message);
                        }
                        break;
                    case '4':
                        try
                        {
                            Console.Clear();
                            char key1 = '0';
                            List<Restriction> blacklist = (List<Restriction>)AesAlg.Decrypt(ReturnBlackList(), secretKey);
                            while (key1 != '3')
                            {
                                int br = 1;

                                Console.WriteLine("Blacklist:");
                                foreach (Restriction r in blacklist)
                                {
                                    Console.WriteLine("{0}. {1}\t{2}\t{3}", br++, r.UserOrGroup, r.Port, r.Protocol);
                                }
                                Console.WriteLine("Chose action:");
                                Console.WriteLine("\t1.Add new restriction");
                                Console.WriteLine("\t2.Delete existing restriction");
                                Console.WriteLine("\t3.Finish");

                                key1 = Console.ReadKey().KeyChar;

                                if (key1 == '1')
                                {
                                    Console.WriteLine("Which user/user group:");
                                    string userG = Console.ReadLine();
                                    Console.WriteLine("Which port");
                                    int portBr = 0;
                                    string port1 = Console.ReadLine();
                                    if (port1 != "")
                                    {
                                        if (!Int32.TryParse(port1, out portBr))
                                        {
                                            Console.WriteLine("For port, please enter a number");
                                            continue;
                                        }

                                    }
                                    string proto = ChoseProto();

                                    Restriction r = new Restriction();
                                    r.UserOrGroup = userG;
                                    r.Port = portBr;
                                    r.Protocol = proto;

                                    blacklist.Add(r);
                                    Console.WriteLine("Restriction successfully added to blacklist!");
                                }
                                else if (key1 == '2')
                                {
                                    int redni = 0;
                                    Console.WriteLine("Wich restriction do you want to delete? Enter the number next to the restriction");
                                    string brisanje = Console.ReadLine();
                                    if (brisanje == "")
                                    {
                                        Console.WriteLine("Please enter a number!");
                                        continue;
                                    }
                                    if (!Int32.TryParse(brisanje, out redni))
                                    {
                                        Console.WriteLine("Please enter a number!");
                                        continue;
                                    }

                                    blacklist.RemoveAt(redni + 1);
                                    Console.WriteLine("Restriction successfully deleted from blackList!");
                                }
                            }
                            if (EditBlackList(AesAlg.Encrypt(Restriction.BlackListToString(blacklist), secretKey)))
                            {
                                Console.WriteLine("Succesfully edited blacklist!");
                            }
                            else
                            {
                                Console.WriteLine("Failed to edit blacklist!");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Request failed! Error message: " + e.Message);
                        }
                        break;
                    case '5':
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
