using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManagement
{
    public class Restriction
    {
        private List<string> user;
        private List<string> group;
        private List<int> port;
        private List<string> protocol;

        public List<string> User { get => user; set => user = value; }
        public List<string> Group { get => group; set => group = value; }
        public List<int> Port { get => port; set => port = value; }
        public List<string> Protocol { get => protocol; set => protocol = value; }
    }
}
