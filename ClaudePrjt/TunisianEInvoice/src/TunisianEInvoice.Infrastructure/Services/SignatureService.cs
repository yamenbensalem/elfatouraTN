using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using TunisianEInvoice.Application.Interfaces;

namespace TunisianEInvoice.Infrastructure.Services
{
    /// <summary>
    /// XML Digital Signature Service for signing invoices.
    /// Implements XAdES-BES signature as required by Tunisian e-invoice regulations.
    /// </summary>
    public class SignatureService : ISignatureService
    {
        public void SignXml(XmlDocument xmlDocument, byte[] certificate, string certificatePassword)
        {
            if (xmlDocument == null)
                throw new ArgumentNullException(nameof(xmlDocument));

            if (certificate == null || certificate.Length == 0)
                throw new ArgumentNullException(nameof(certificate));

            // Load the certificate
            var cert = new X509Certificate2(certificate, certificatePassword, 
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

            if (!cert.HasPrivateKey)
                throw new InvalidOperationException("Certificate does not contain a private key");

            var privateKey = cert.GetRSAPrivateKey();
            if (privateKey == null)
                throw new InvalidOperationException("Unable to get RSA private key from certificate");

            // Create SignedXml object
            var signedXml = new SignedXml(xmlDocument)
            {
                SigningKey = privateKey
            };

            // Create a reference to be signed
            var reference = new Reference
            {
                Uri = "" // Sign the entire document
            };

            // Add an enveloped transformation to the reference
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add C14N transformation (required for XAdES)
            var c14n = new XmlDsigC14NTransform();
            reference.AddTransform(c14n);

            // Add the reference to the SignedXml object
            signedXml.AddReference(reference);

            // Add KeyInfo (certificate information)
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            // Compute the signature
            signedXml.ComputeSignature();

            // Get the XML representation of the signature
            var xmlDigitalSignature = signedXml.GetXml();

            // Append the signature to the XML document
            xmlDocument.DocumentElement?.AppendChild(xmlDocument.ImportNode(xmlDigitalSignature, true));

            // TODO: Add XAdES-BES specific elements:
            // - SigningTime
            // - SigningCertificate
            // - SignaturePolicyIdentifier (with TTN policy OID)
            // This requires additional implementation for full XAdES compliance
        }

        public bool VerifySignature(XmlDocument xmlDocument)
        {
            if (xmlDocument == null)
                throw new ArgumentNullException(nameof(xmlDocument));

            // Find the Signature element
            var nodeList = xmlDocument.GetElementsByTagName("Signature");
            
            if (nodeList.Count == 0)
                return false;

            var signedXml = new SignedXml(xmlDocument);
            signedXml.LoadXml((XmlElement)nodeList[0]);

            return signedXml.CheckSignature();
        }
    }
}
