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
            if (msg == "TryConnect")
                return "Success!";
            if (Program.list == null)
            {
                Program.list = new List<MessageFromSM>();
            }
            //Console.WriteLine("Communication established.");
            MessageFromSM mfSM = new MessageFromSM();
            string[] arr = msg.Split('|');

            if (Program.list.Count == 0)
            {
                mfSM.ImeMasine = arr[0];
                mfSM.Protokol = arr[1];
                mfSM.Port = Int32.Parse(arr[2]);
                ++mfSM.CounterForDOS;
                Program.list.Add(mfSM);
            }
            else
            {
                foreach (var item in Program.list)
                {
                    mfSM = item;
                    if (item.CounterForDOS == 10)
                    {
                        return "DoS";
                    }
                    if (item.ImeMasine == arr[0])
                    {
                        mfSM.ImeMasine = arr[0];
                        mfSM.Protokol = arr[1];
                        mfSM.Port = Int32.Parse(arr[2]);
                        ++mfSM.CounterForDOS;
                    }
                    else
                    {
                        mfSM.ImeMasine = arr[0];
                        mfSM.Protokol = arr[1];
                        mfSM.Port = Int32.Parse(arr[2]);
                        ++mfSM.CounterForDOS;
                    }
                }
            }
            return mfSM.ImeMasine + $" attemp:{mfSM.CounterForDOS}";
        }
        
    }
}
