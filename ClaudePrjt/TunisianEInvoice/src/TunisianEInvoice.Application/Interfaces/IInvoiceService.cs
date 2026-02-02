using System.Threading.Tasks;
using TunisianEInvoice.Application.DTOs;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceResponseDto> GenerateInvoiceAsync(InvoiceRequestDto request);
        Task<string> GenerateXmlWithoutSignatureAsync(InvoiceRequestDto request);
        Task<ValidationResultDto> ValidateInvoiceAsync(InvoiceRequestDto request);
        Task<byte[]> GeneratePdfAsync(InvoiceRequestDto request);
    }
}
