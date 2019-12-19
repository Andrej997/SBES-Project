using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TestiranjeOtvorenihKonekcija
{
    class Program
    {
        static void Main(string[] args)
        {
            string machineName = System.Environment.MachineName;
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:10000/" + machineName;
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            

            IWCFContract factory = new ChannelFactory<IWCFContract>(binding, address).CreateChannel();

            factory.Connect("");

            EndpointAddress endpointAddress = new EndpointAddress(new Uri(address));
        }
    }
}
