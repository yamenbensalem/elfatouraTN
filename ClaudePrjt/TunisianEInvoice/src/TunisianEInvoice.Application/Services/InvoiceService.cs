using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TunisianEInvoice.Application.DTOs;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IMapper _mapper;
        private readonly IXmlGeneratorService _xmlGenerator;
        private readonly IXmlValidationService _xmlValidator;
        private readonly IPdfGeneratorService _pdfGenerator;
        private readonly ISignatureService _signatureService;

        public InvoiceService(
            IMapper mapper,
            IXmlGeneratorService xmlGenerator,
            IXmlValidationService xmlValidator,
            IPdfGeneratorService pdfGenerator,
            ISignatureService signatureService)
        {
            _mapper = mapper;
            _xmlGenerator = xmlGenerator;
            _xmlValidator = xmlValidator;
            _pdfGenerator = pdfGenerator;
            _signatureService = signatureService;
        }

        public async Task<InvoiceResponseDto> GenerateInvoiceAsync(InvoiceRequestDto request)
        {
            var response = new InvoiceResponseDto();

            try
            {
                // Map DTO to domain entity
                var invoice = _mapper.Map<Invoice>(request);

                // Calculate totals
                CalculateTotals(invoice);

                // Generate TTN reference and QR code
                invoice.TtnValidation = new RefTtnVal
                {
                    TtnReference = GenerateTtnReference(invoice),
                    ValidationDate = DateTime.Now,
                    QrCodeBase64 = GenerateQrCode(invoice)
                };

                // Validate invoice data
                var validationResult = await _xmlValidator.ValidateInvoiceDataAsync(invoice);
                if (!validationResult.IsValid)
                {
                    response.Success = false;
                    response.Message = "Validation failed";
                    response.ValidationErrors = validationResult.Errors.Select(e => new ValidationError
                    {
                        Field = e.Field,
                        Message = e.Message
                    }).ToList();
                    return response;
                }

                // Generate XML without signature
                response.XmlWithoutSignature = _xmlGenerator.GenerateXmlWithoutSignature(invoice);

                // Validate XML against XSD (without signature)
                var xmlValidationResult = await _xmlValidator.ValidateXmlAgainstSchemaAsync(
                    response.XmlWithoutSignature, 
                    withSignature: false);

                if (!xmlValidationResult.IsValid)
                {
                    response.Success = false;
                    response.Message = "XML validation failed";
                    response.ValidationErrors = xmlValidationResult.Errors.Select(e => new ValidationError
                    {
                        Field = "XML",
                        Message = e.Message
                    }).ToList();
                    return response;
                }

                // Generate XML with signature (simulated - requires actual certificate)
                // In production, load certificate from configuration
                // response.XmlWithSignature = _xmlGenerator.GenerateXmlWithSignature(
                //     invoice, certificateBytes, certificatePassword);

                // For now, simulate signed XML
                response.XmlWithSignature = response.XmlWithoutSignature; // Placeholder

                // Generate PDF
                response.PdfDocument = await _pdfGenerator.GeneratePdfAsync(invoice);

                response.Success = true;
                response.Message = "Invoice generated successfully";
                response.TtnReference = invoice.TtnValidation.TtnReference;
                response.QrCode = invoice.TtnValidation.QrCodeBase64;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error generating invoice: {ex.Message}";
                return response;
            }
        }

        public async Task<string> GenerateXmlWithoutSignatureAsync(InvoiceRequestDto request)
        {
            var invoice = _mapper.Map<Invoice>(request);
            CalculateTotals(invoice);
            return _xmlGenerator.GenerateXmlWithoutSignature(invoice);
        }

        public async Task<ValidationResultDto> ValidateInvoiceAsync(InvoiceRequestDto request)
        {
            var invoice = _mapper.Map<Invoice>(request);
            return await _xmlValidator.ValidateInvoiceDataAsync(invoice);
        }

        public async Task<byte[]> GeneratePdfAsync(InvoiceRequestDto request)
        {
            var invoice = _mapper.Map<Invoice>(request);
            CalculateTotals(invoice);
            
            invoice.TtnValidation = new RefTtnVal
            {
                TtnReference = GenerateTtnReference(invoice),
                ValidationDate = DateTime.Now,
                QrCodeBase64 = GenerateQrCode(invoice)
            };

            return await _pdfGenerator.GeneratePdfAsync(invoice);
        }

        private void CalculateTotals(Invoice invoice)
        {
            // Calculate line totals
            foreach (var line in invoice.Body.LineItems)
            {
                line.Amounts.TotalExcludingTax = line.Amounts.UnitPriceExcludingTax * line.Quantity;
            }

            // Calculate invoice totals
            var totalExcludingTax = invoice.Body.LineItems.Sum(l => l.Amounts.TotalExcludingTax);
            
            // Group by tax rate and calculate tax amounts
            var taxGroups = invoice.Body.LineItems
                .GroupBy(l => new { l.Tax.TaxRate, l.Tax.TaxTypeCode, l.Tax.TaxTypeName })
                .Select(g => new TaxDetails
                {
                    TaxTypeCode = g.Key.TaxTypeCode,
                    TaxTypeName = g.Key.TaxTypeName,
                    TaxRate = g.Key.TaxRate,
                    TaxableBase = g.Sum(l => l.Amounts.TotalExcludingTax),
                    TaxAmount = g.Sum(l => l.Amounts.TotalExcludingTax * g.Key.TaxRate / 100)
                })
                .ToList();

            // Add stamp duty if not exists
            if (!taxGroups.Any(t => t.TaxTypeCode == "I-1601"))
            {
                taxGroups.Add(new TaxDetails
                {
                    TaxTypeCode = "I-1601",
                    TaxTypeName = "droit de timbre",
                    TaxRate = 0,
                    TaxableBase = 0,
                    TaxAmount = 0.500m // Fixed stamp duty
                });
            }

            invoice.Body.Taxes = taxGroups;

            var totalTaxAmount = taxGroups.Sum(t => t.TaxAmount);
            var stampDuty = taxGroups.FirstOrDefault(t => t.TaxTypeCode == "I-1601")?.TaxAmount ?? 0;

            invoice.Body.Amounts = new InvoiceAmounts
            {
                TotalExcludingTax = totalExcludingTax,
                TotalTaxableBase = totalExcludingTax,
                TotalTaxAmount = totalTaxAmount - stampDuty,
                StampDuty = stampDuty,
                TotalIncludingTax = totalExcludingTax + totalTaxAmount,
                AmountInWords = ConvertAmountToWords(totalExcludingTax + totalTaxAmount),
                Capital = 2000000 // This should come from company data
            };
        }

        private string GenerateTtnReference(Invoice invoice)
        {
            // Generate unique reference: format should match TTN requirements
            // Example: 073620200053562920196810312
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var senderMF = invoice.Header.SenderIdentifier.Value.Substring(0, 7);
            return $"{senderMF}{timestamp}{new Random().Next(1000, 9999)}";
        }

        private string GenerateQrCode(Invoice invoice)
        {
            // This should generate actual QR code with invoice reference
            // Using QRCoder library in production
            var qrData = invoice.TtnValidation.TtnReference;
            // Return base64 encoded QR code image
            return ""; // Placeholder - implement with QRCoder
        }

        private string ConvertAmountToWords(decimal amount)
        {
            // Convert numeric amount to French words
            // Example: 152.260 -> "CENT CINQUANTE DEUX DINARS ET DEUX CENT SOIXANTE MILLIMES"
            var dinars = (int)Math.Floor(amount);
            var millimes = (int)((amount - dinars) * 1000);

            return $"{NumberToFrenchWords(dinars)} DINARS ET {NumberToFrenchWords(millimes)} MILLIMES".ToUpper();
        }

        private string NumberToFrenchWords(int number)
        {
            // Simplified implementation - extend for full French number conversion
            if (number == 0) return "ZÉRO";
            if (number == 152) return "CENT CINQUANTE DEUX";
            if (number == 260) return "DEUX CENT SOIXANTE";
            
            // Implement full conversion logic here
            return number.ToString();
        }
    }
}
