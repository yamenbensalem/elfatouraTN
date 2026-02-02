using System.Xml;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface ISignatureService
    {
        void SignXml(XmlDocument xmlDocument, byte[] certificate, string certificatePassword);
        bool VerifySignature(XmlDocument xmlDocument);
    }
}
