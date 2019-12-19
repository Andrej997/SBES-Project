using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;

namespace AuditContracts
{
    public class AuditCertValidator : X509CertificateValidator
    {
        public override void Validate(X509Certificate2 certificate)
        {
            if (certificate.Subject.Equals(certificate.Issuer))
            {
                throw new Exception("Certificate is self-issued.");
            }
        }
    }
}
