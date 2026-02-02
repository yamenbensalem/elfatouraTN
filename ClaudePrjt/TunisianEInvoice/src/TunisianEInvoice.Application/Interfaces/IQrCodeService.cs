namespace TunisianEInvoice.Application.Interfaces
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Generates a QR code image as a byte array from the provided data
        /// </summary>
        /// <param name="data">The data to encode in the QR code</param>
        /// <param name="pixelsPerModule">Size of each module in pixels (default: 20)</param>
        /// <returns>PNG image as byte array</returns>
        byte[] GenerateQrCode(string data, int pixelsPerModule = 20);

        /// <summary>
        /// Generates a QR code as a Base64-encoded PNG string
        /// </summary>
        /// <param name="data">The data to encode in the QR code</param>
        /// <param name="pixelsPerModule">Size of each module in pixels (default: 20)</param>
        /// <returns>Base64 string of the PNG image</returns>
        string GenerateQrCodeBase64(string data, int pixelsPerModule = 20);

        /// <summary>
        /// Generates the El Fatoora QR code content string from invoice data
        /// Format: Matricule Fiscal|Numéro Facture|Date Facture|Total TTC
        /// </summary>
        string BuildElFatooraQrContent(string matriculeFiscal, string invoiceNumber, DateTime invoiceDate, decimal totalTtc);
    }
}
