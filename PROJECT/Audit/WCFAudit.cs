using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AuditContracts;

namespace Audit
{
    public class WCFAudit : IWCFAudit
    {
        public string ConnectS(string msg)
        {
            Console.WriteLine("Communication established.");
            return "Hello";
        }
        
    }
}
