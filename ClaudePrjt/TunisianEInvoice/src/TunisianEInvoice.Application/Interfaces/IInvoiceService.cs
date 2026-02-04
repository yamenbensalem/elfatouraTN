using System.Threading.Tasks;
using TunisianEInvoice.Application.DTOs;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceResponseDto> GenerateInvoiceAsync(InvoiceRequestDto request);
        Task<string> GenerateXmlWithoutSignatureAsync(InvoiceRequestDto request);
        Task<ValidationResultDto> ValidateInvoiceAsync(InvoiceRequestDto request);
        Task<byte[]> GeneratePdfAsync(InvoiceRequestDto request);
        Task<byte[]> RegeneratePdfFromInvoiceAsync(InvoiceRecord invoiceRecord);
    }
}
