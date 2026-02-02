using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface IXmlGeneratorService
    {
        string GenerateXmlWithoutSignature(Invoice invoice);
        string GenerateXmlWithSignature(Invoice invoice, byte[] certificate, string certificatePassword);
    }
}
