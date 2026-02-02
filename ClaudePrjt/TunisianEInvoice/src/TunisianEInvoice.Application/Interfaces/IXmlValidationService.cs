using System.Threading.Tasks;
using TunisianEInvoice.Application.DTOs;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface IXmlValidationService
    {
        Task<ValidationResultDto> ValidateInvoiceDataAsync(Invoice invoice);
        Task<ValidationResultDto> ValidateXmlAgainstSchemaAsync(string xml, bool withSignature);
    }
}
