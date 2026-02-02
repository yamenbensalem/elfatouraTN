using QRCoder;
using TunisianEInvoice.Application.Interfaces;

namespace TunisianEInvoice.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GenerateQrCode(string data, int pixelsPerModule = 20)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule);
        }

        public string GenerateQrCodeBase64(string data, int pixelsPerModule = 20)
        {
            var imageBytes = GenerateQrCode(data, pixelsPerModule);
            return Convert.ToBase64String(imageBytes);
        }

        public string BuildElFatooraQrContent(string matriculeFiscal, string invoiceNumber, DateTime invoiceDate, decimal totalTtc)
        {
            // El Fatoora QR code format: MF|NumFac|Date|TotalTTC
            // Date format: DD/MM/YYYY
            // Amount format: decimal with 3 places
            var dateStr = invoiceDate.ToString("dd/MM/yyyy");
            var amountStr = totalTtc.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
            
            return $"{matriculeFiscal}|{invoiceNumber}|{dateStr}|{amountStr}";
        }
    }
}
