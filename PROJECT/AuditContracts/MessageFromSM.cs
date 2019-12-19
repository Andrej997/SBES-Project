using System;

namespace AuditContracts
{
    public class MessageFromSM
    {
        // ime klijenta
        private string imeMasine; 
        // port na kojem zeli da se konektuje
        private int port;
        // protokol preko kojeg zeli da se konetkuje
        private string protokol;
        // brojac zabranjenih pristupa
        private int counterForDOS;
        // vreme konektovanja
        private DateTime connectTime;

        public MessageFromSM()
        {
            CounterForDOS = 0;
        }
        public string ImeMasine { get => imeMasine; set => imeMasine = value; }
        public int Port { get => port; set => port = value; }
        public string Protokol { get => protokol; set => protokol = value; }
        public int CounterForDOS { get => counterForDOS; set => counterForDOS = value; }
        public DateTime ConnectTime { get => connectTime; set => connectTime = value; }
    }
}
