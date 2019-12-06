using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Contracts;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Diagnostics;

namespace ServiceManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            Process notePad = new Process();
            notePad.StartInfo.FileName = "notepad.exe";
            //notePad.StartInfo.Arguments = "mytextfile.txt";
            notePad.Start();
        }
    }
}
