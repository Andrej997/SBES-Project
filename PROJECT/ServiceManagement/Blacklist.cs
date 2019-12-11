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
        public Blacklist()
        {
            WriteInXml();
        }

        public bool WriteInXml()
        {
            bool success = false;
            try
            {
                XmlWriter xmlWriter = XmlWriter.Create("Blacklist.xml");

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Black-list");

                xmlWriter.WriteStartElement("restriction1");
                xmlWriter.WriteAttributeString("group", "Jadnici");
                xmlWriter.WriteAttributeString("port", "5214");
                xmlWriter.WriteAttributeString("protocol", "UDP");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("restriction2");
                xmlWriter.WriteAttributeString("group", "Umetnici");
                xmlWriter.WriteAttributeString("port", "12045");
                xmlWriter.WriteAttributeString("protocol", "FTP");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("restriction3");
                xmlWriter.WriteAttributeString("group", "Genijalci");
                xmlWriter.WriteAttributeString("port", "8624");
                xmlWriter.WriteAttributeString("protocol", "TCP");
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



    }


}
