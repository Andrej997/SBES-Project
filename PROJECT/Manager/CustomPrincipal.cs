using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class CustomPrincipal : IPrincipal
    {
        WindowsIdentity identity = null;

        public CustomPrincipal(WindowsIdentity windowsIdentity)
        {
            identity = windowsIdentity;
        }
        public IIdentity Identity
        {
            get { return identity; }
        }


        public bool IsInRole(string dozvola)
        {
            foreach (IdentityReference grupa in this.identity.Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)grupa.Translate(typeof(SecurityIdentifier));
                var ime = sid.Translate(typeof(NTAccount));
                string imeGrupe = Formatter.ParseName(ime.ToString());
                string[] dozvole;
                //pozivamo metodu koja nam vraca sve dozvole iz RoleConfigFile.resx za datu grupu, ako dozvole ne postoje, metoda vraca false
                if (RoleConfig.GetPermissions(imeGrupe, out dozvole))
                {
                    foreach (string doz in dozvole)
                    {
                        if (doz.Equals(dozvola))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
