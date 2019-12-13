using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditContracts
{
    public class MessageFromSM
    {
        private string imeMasine;
        private int port;
        private string protokol;
        private int counterForDOS;

        public MessageFromSM()
        {
            CounterForDOS = 0;
        }
        public string ImeMasine { get => imeMasine; set => imeMasine = value; }
        public int Port { get => port; set => port = value; }
        public string Protokol { get => protokol; set => protokol = value; }
        public int CounterForDOS { get => counterForDOS; set => counterForDOS = value; }
    }
}
