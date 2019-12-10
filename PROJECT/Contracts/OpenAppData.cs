using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [Serializable]
    public class OpenAppData
    {
        private string imeMasine;
        private int port;
        private string protokol;

        public OpenAppData(string ime, int port, string protokol)
        {
            ImeMasine = ime;
            Port = port;
            Protokol = protokol;
        }
        public string ImeMasine { get => imeMasine; set => imeMasine = value; }
        public int Port { get => port; set => port = value; }
        public string Protokol { get => protokol; set => protokol = value; }
    }
}
