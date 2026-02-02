using System.Collections.Generic;

namespace TunisianEInvoice.Application.DTOs
{
    public class InvoiceResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string XmlWithoutSignature { get; set; }
        public string XmlWithSignature { get; set; }
        public byte[] PdfDocument { get; set; }
        public string TtnReference { get; set; }
        public string QrCode { get; set; }
        public List<ValidationError> ValidationErrors { get; set; }
    }
}
