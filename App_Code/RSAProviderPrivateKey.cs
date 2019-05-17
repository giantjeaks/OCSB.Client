using iTextSharp.text.pdf.security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LTVDigitalSignature
{
    class RSAProviderPrivateKey : IExternalSignature
    {
        private RSACryptoServiceProvider privateKey;
        private HashAlgorithm hashAlgorithm;
        private String hashAlgorithmName;

        public RSAProviderPrivateKey(X509Certificate2 eeCertificate, String hashAlgorithm)
        {
            this.hashAlgorithm = HashAlgorithm.Create(hashAlgorithm);
            this.hashAlgorithmName = hashAlgorithm;

            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)eeCertificate.PrivateKey;

            //var enhCsp = new RSACryptoServiceProvider().CspKeyContainerInfo;
            var enhCsp = csp.CspKeyContainerInfo;
            var cspparams = new CspParameters(enhCsp.ProviderType, enhCsp.ProviderName, csp.CspKeyContainerInfo.KeyContainerName);

            cspparams.Flags = CspProviderFlags.UseUserProtectedKey;

            this.privateKey = new RSACryptoServiceProvider(cspparams);
        }

        public string GetEncryptionAlgorithm()
        {
            return "RSA";
        }

        public string GetHashAlgorithm()
        {
            return this.hashAlgorithmName;
        }

        public byte[] Sign(byte[] message)
        {
            return privateKey.SignData(message, this.hashAlgorithm);
        }
    }
}
