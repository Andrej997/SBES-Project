using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace ServiceManagement
{
    public class Blacklist
    {

        /*public Blacklist()
        {
            //WriteInXml();
        }

        public bool WriteInXml()
        {
            bool success = false;
            try
            {
                XmlWriter xmlWriter = XmlWriter.Create("Blacklist.xml");

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Black-list");

                xmlWriter.WriteStartElement("restriction");
                xmlWriter.WriteStartElement("user");
                xmlWriter.WriteString("Jadnik1, Jadnik2");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("group");
                xmlWriter.WriteString("Jadnici");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("port");
                xmlWriter.WriteString("5214");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("protocol");
                xmlWriter.WriteString("UDP");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();


                xmlWriter.WriteStartElement("restriction");
                xmlWriter.WriteStartElement("user");
                xmlWriter.WriteString("Umetnik1234");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("group");
                xmlWriter.WriteString("Umetnici");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("port");
                xmlWriter.WriteString("12045");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("protocol");
                xmlWriter.WriteString("");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

              

                xmlWriter.WriteStartElement("restriction");
                xmlWriter.WriteStartElement("user");
                xmlWriter.WriteString("GenijalacXD");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("group");
                xmlWriter.WriteString("Genijalci");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("port");
                xmlWriter.WriteString("8624");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("protocol");
                xmlWriter.WriteString("TCP, UDP");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return success;
        }

        public bool WriteInXml(List<Restriction> list)
        {
            bool success = false;
            try
            {
                XmlWriter xmlWriter = XmlWriter.Create("BlacklistParam.xml");

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Black-list");

                foreach (var restriction in list)
                {
                    xmlWriter.WriteStartElement("restriction");
                    xmlWriter.WriteStartElement("user");
                    string str = "";
                    int users = restriction.User.Count();
                    foreach (var user in restriction.User)
                    {
                        if (--users >= 1)
                            str += user + ',';
                        else
                            str += user;
                    }
                    xmlWriter.WriteString(str);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("group");
                    string str1 = "";
                    int groups = restriction.Group.Count();
                    foreach (var group in restriction.Group)
                    {
                        if (--groups >= 1)
                            str1 += group + ',';
                        else
                            str1 += group;
                    }
                    xmlWriter.WriteString(str1);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("port");
                    string str2 = "";
                    int ports = restriction.Port.Count();
                    foreach (var port in restriction.Port)
                    {
                        if (--ports >= 1)
                            str2 += port + ',';
                        else
                            str2 += port;
                    }
                    xmlWriter.WriteString(str2);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("protocol");
                    string str3 = "";
                    int protocols = restriction.Protocol.Count();
                    foreach (var protocol in restriction.Protocol)
                    {
                        if (--protocols >= 1)
                            str3 += protocol + ',';
                        else
                            str3 += protocol;
                    }
                    xmlWriter.WriteString(str3);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
                


                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return success;
        }

        public List<Restriction> ReadFromXml()
        {
            Restriction rest = null;
            List<Restriction> list = new List<Restriction>();

            try
            {
                XmlReader xmlReader = XmlReader.Create("Blacklist.xml");
                while (xmlReader.Read())
                {
                    // Only detect start elements.
                    if (xmlReader.IsStartElement())
                    {
                        // Get element name and switch on it.
                        switch (xmlReader.Name)
                        {
                            case "restriction":
                                // Detect this element.
                                //Console.WriteLine("Start <restriction> element.");
                                rest = new Restriction();
                                rest.User = new List<string>();
                                rest.Group = new List<string>();
                                rest.Port = new List<int>();
                                rest.Protocol = new List<string>();
                                break;
                            case "user":
                                // Detect this article element.
                                //Console.WriteLine("Start <port> element.");
                                // Search for the attribute name on this current node.
                                string attribute = xmlReader["user"];
                                if (attribute != null)
                                {
                                    //Console.WriteLine("  Has attribute name: " + attribute);
                                }
                                // Next read will contain text.
                                if (xmlReader.Read())
                                {
                                    //Console.WriteLine("  Text node: " + xmlReader.Value.Trim());
                                    string str = xmlReader.Value.Trim();
                                    string[] niz = str.Split(',');
                                    for (int i = 0; i < niz.Length; i++)
                                    {
                                        if (niz[i] != "")
                                            rest.User.Add(niz[i]);
                                    }
                                }
                                break;
                            case "group":
                                // Detect this article element.
                                //Console.WriteLine("Start <group> element.");
                                // Search for the attribute name on this current node.
                                string attributeGroup = xmlReader["port"];
                                if (attributeGroup != null)
                                {
                                    //Console.WriteLine("  Has attribute name: " + attributeGroup);
                                }
                                // Next read will contain text.
                                if (xmlReader.Read())
                                {
                                    //Console.WriteLine("  Text node: " + xmlReader.Value.Trim());
                                    string str = xmlReader.Value.Trim();
                                    string[] niz = str.Split(',');
                                    for (int i = 0; i < niz.Length; i++)
                                    {
                                        if (niz[i] != "")
                                            rest.Group.Add(niz[i]);
                                    }
                                }
                                break;
                            case "port":
                                // Detect this article element.
                                //Console.WriteLine("Start <port> element.");
                                // Search for the attribute name on this current node.
                                string attributePort = xmlReader["port"];
                                if (attributePort != null)
                                {
                                    //Console.WriteLine("  Has attribute name: " + attributePort);
                                }
                                // Next read will contain text.
                                if (xmlReader.Read())
                                {
                                    //Console.WriteLine("  Text node: " + xmlReader.Value.Trim());
                                    string str = xmlReader.Value.Trim();
                                    string[] niz = str.Split(',');
                                    for (int i = 0; i < niz.Length; i++)
                                    {
                                        if(niz[i] != "")
                                            rest.Port.Add(Int32.Parse(niz[i]));
                                    }
                                }
                                break;
                            case "protocol":
                                // Detect this article element.
                                //Console.WriteLine("Start <protocol> element.");
                                // Search for the attribute name on this current node.
                                string attributeProtocol = xmlReader["protocol"];
                                if (attributeProtocol != null)
                                {
                                    //Console.WriteLine("  Has attribute name: " + attributeProtocol);
                                }
                                // Next read will contain text.
                                if (xmlReader.Read())
                                {
                                    //Console.WriteLine("  Text node: " + xmlReader.Value.Trim());
                                    string str = xmlReader.Value.Trim();
                                    string[] niz = str.Split(',');
                                    for (int i = 0; i < niz.Length; i++)
                                    {
                                        if (niz[i] != "")
                                            rest.Protocol.Add(niz[i]);
                                    }
                                    list.Add(rest);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            //Console.ReadKey();

            return list;
        }*/

    }


}
