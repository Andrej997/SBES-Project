using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AuditContracts;

namespace Audit
{
    public class WCFAudit : IWCFAudit
    {
        private static EventLog customLog = null;
        const string SourceName = "Audit";
        const string LogName = "ProjectLog";
        public string ConnectS(string msg)
        {
            if (msg == "TryConnect")
                return "Success!";
            

            if (!EventLog.SourceExists(SourceName))
            {
                EventLog.CreateEventSource(SourceName, LogName);
            }
            customLog = new EventLog(LogName, Environment.MachineName, SourceName);

            if (Program.list == null)
            {
                Program.list = new List<MessageFromSM>();
            }

            MessageFromSM mfSM = new MessageFromSM();
            string[] arr = msg.Split('|');
            string clientName = arr[0];
            string clientProtocol = arr[1];
            string clientPort = arr[2];
            
            mfSM.ImeMasine = arr[0];
            mfSM.Protokol = arr[1];
            mfSM.Port = Int32.Parse(arr[2]);

            if (Program.list.Count == 0)
            {
                ++mfSM.CounterForDOS;
                LogEvent(mfSM, EventLogEntryType.Warning);
                mfSM.ConnectTime = DateTime.Now;
                Program.list.Add(mfSM);
            }
            else
            {
                bool found = false;
                foreach (var item in Program.list)
                {
                    if (item.ImeMasine == clientName)
                    {
                        found = true;
                        ++item.CounterForDOS;
                        if (CheckDoS(item.CounterForDOS))
                        {
                            TimeSpan dt = DateTime.Now - item.ConnectTime;
                            if (dt.Minutes < Program.paramsForDoS.Item2) // ako je napravio odredjen broj prekrsaja u intervalu do 10 min tek onda ide dos
                                return LogEvent(item, EventLogEntryType.Error);
                            else
                            { // postoje je vremenski interval prosao, clijentu se nulira counter
                                item.CounterForDOS = 1;
                                LogEvent(item, EventLogEntryType.Warning);
                            }
                        }
                        else
                        {
                            LogEvent(item, EventLogEntryType.Warning);
                            break;
                        }
                    }
                }
                if (found == false) // ako ga nije nasao
                {
                    ++mfSM.CounterForDOS;
                    mfSM.ConnectTime = DateTime.Now;
                    Program.list.Add(mfSM);
                    LogEvent(mfSM, EventLogEntryType.Warning);
                }
                
            }
            
            return mfSM.ImeMasine + $" attemp:{mfSM.CounterForDOS}";
        }

        private static bool CheckDoS(int count)
        {
            if (count == Program.paramsForDoS.Item1)
                return true;
            else
                return false;
        }

        private static string LogEvent(MessageFromSM item, EventLogEntryType eventLog)
        {
            string message = string.Format($"{item.ImeMasine} denied {item.CounterForDOS} times!", item.ImeMasine, OperationContext.Current.IncomingMessageHeaders.Action, "DoS");
            customLog.WriteEntry(message, eventLog);
            if (eventLog == EventLogEntryType.Error) // ako je poslat Error znaci da je DoS u pitanju i izbaci ga iz liste radi memorije
            {
                Program.list.Remove(item);
            }
            return "DoS";
        }
        
        
    }
}
