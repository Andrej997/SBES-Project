using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Contracts
{
    [Serializable]
    public class Restriction
    {
        /*private List<string> user;
        private List<string> group;
        private List<int> port;
        private List<string> protocol;

        public List<string> User { get => user; set => user = value; }
        public List<string> Group { get => group; set => group = value; }
        public List<int> Port { get => port; set => port = value; }
        public List<string> Protocol { get => protocol; set => protocol = value; }*/

        private string userOrGroup;
        private int port;
        private string protocol;

        public string UserOrGroup { get => userOrGroup; set => userOrGroup = value; }
        public int Port { get => port; set => port = value; }
        public string Protocol { get => protocol; set => protocol = value; }


        public static void WriteBlackList(List<Restriction> lista)
        {
            XmlSerializer writer = new XmlSerializer(typeof(List<Restriction>));
            FileStream file = File.OpenWrite("BlackList.xml");
            writer.Serialize(file, lista);
            file.Close();
        }

        public static List<Restriction> ReadBlackList()
        {
            XmlSerializer reader = new XmlSerializer(typeof(List<Restriction>));
            FileStream file = File.OpenRead("BlackList.xml");
            var objects = reader.Deserialize(file);
            file.Close();
            return (List<Restriction>)objects;
        }


        public static bool IsRestricted(List<Restriction> blackList, OpenAppData data, string user, List<string> userGroups)
        {
            foreach (Restriction res in blackList)
            {
                if (user == res.UserOrGroup)
                {
                    if (data.Port == res.Port || data.Protokol == res.Protocol)
                        return true;
                }

                foreach (string s in userGroups)
                {
                    string[] temp = null;
                    try
                    { // posto su neke grupe u obliku DESKTOP-...\...
                        temp = s.Split('\\');
                        temp[0] = temp[1];
                    }
                    catch
                    {
                        temp[0] = s;
                    }
                    if (temp[0] == res.UserOrGroup)
                    {
                        if (data.Port == res.Port || data.Protokol == res.Protocol)
                            return true;
                    }
                }
            }

            return false;
        }

        public static string BlackListToString(List<Restriction> list)
        {
            string returnS = "";

            foreach (Restriction s in list)
            {
                returnS += string.Format("{0},{1},{2},", s.UserOrGroup, s.Port, s.Protocol);
            }

            returnS += "12345678312313212313213123";
            return returnS;
        }
    }
}
