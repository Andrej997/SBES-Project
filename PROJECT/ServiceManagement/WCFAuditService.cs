using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuditContracts;

namespace ServiceManagement
{
    public class WCFAuditService : IWCFAudit
    {
        public void Connect(string msg)
        {
            Console.WriteLine("Communication established.");
        }
    }
}
