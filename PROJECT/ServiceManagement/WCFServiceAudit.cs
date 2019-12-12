using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuditContracts;

namespace ServiceManagement
{
    public class WCFServiceAudit : IWCFAudit
    {
        public void Connect(string msg)
        {
            Console.WriteLine("Communication established.");
        }
    }
}
