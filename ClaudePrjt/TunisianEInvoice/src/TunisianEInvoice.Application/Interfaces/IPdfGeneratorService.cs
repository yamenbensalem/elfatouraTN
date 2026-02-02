using System.Threading.Tasks;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface IPdfGeneratorService
    {
        Task<byte[]> GeneratePdfAsync(Invoice invoice);
    }
}
