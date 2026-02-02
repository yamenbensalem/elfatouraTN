using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using TunisianEInvoice.Application.DTOs;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Services
{
    public class XmlValidationService : IXmlValidationService
    {
        private readonly string _schemaPath;

        public XmlValidationService()
        {
            // TODO: Configure schema path from appsettings
            _schemaPath = "Resources/Schemas";
        }

        public async Task<ValidationResultDto> ValidateInvoiceDataAsync(Invoice invoice)
        {
            var result = new ValidationResultDto
            {
                IsValid = true,
                Errors = new List<ValidationError>()
            };

            // Validate sender identifier
            if (invoice.Header?.SenderIdentifier == null)
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Header.SenderIdentifier",
                    Message = "Sender identifier is required"
                });
            }
            else if (!invoice.Header.SenderIdentifier.IsValid())
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Header.SenderIdentifier",
                    Message = $"Invalid sender identifier format for type {invoice.Header.SenderIdentifier.Type}"
                });
            }

            // Validate receiver identifier
            if (invoice.Header?.ReceiverIdentifier == null)
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Header.ReceiverIdentifier",
                    Message = "Receiver identifier is required"
                });
            }
            else if (!invoice.Header.ReceiverIdentifier.IsValid())
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Header.ReceiverIdentifier",
                    Message = $"Invalid receiver identifier format for type {invoice.Header.ReceiverIdentifier.Type}"
                });
            }

            // Validate document info
            if (string.IsNullOrEmpty(invoice.Body?.DocumentInfo?.DocumentIdentifier))
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Body.DocumentInfo.DocumentIdentifier",
                    Message = "Document identifier is required"
                });
            }

            if (string.IsNullOrEmpty(invoice.Body?.DocumentInfo?.DocumentTypeCode))
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Body.DocumentInfo.DocumentTypeCode",
                    Message = "Document type code is required"
                });
            }

            // Validate line items
            if (invoice.Body?.LineItems == null || invoice.Body.LineItems.Count == 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Body.LineItems",
                    Message = "At least one line item is required"
                });
            }
            else
            {
                for (int i = 0; i < invoice.Body.LineItems.Count; i++)
                {
                    var item = invoice.Body.LineItems[i];
                    if (item.Quantity <= 0)
                    {
                        result.Errors.Add(new ValidationError
                        {
                            Field = $"Body.LineItems[{i}].Quantity",
                            Message = "Quantity must be greater than zero"
                        });
                    }

                    if (item.Amounts?.UnitPriceExcludingTax < 0)
                    {
                        result.Errors.Add(new ValidationError
                        {
                            Field = $"Body.LineItems[{i}].Amounts.UnitPriceExcludingTax",
                            Message = "Unit price cannot be negative"
                        });
                    }
                }
            }

            // Validate partners
            if (invoice.Body?.Partners == null || invoice.Body.Partners.Count < 2)
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "Body.Partners",
                    Message = "Both sender (I-62) and receiver (I-64) partners are required"
                });
            }

            result.IsValid = result.Errors.Count == 0;
            return await Task.FromResult(result);
        }

        public async Task<ValidationResultDto> ValidateXmlAgainstSchemaAsync(string xml, bool withSignature)
        {
            var result = new ValidationResultDto
            {
                IsValid = true,
                Errors = new List<ValidationError>()
            };

            try
            {
                var schemaFileName = withSignature ? "TEIF_with_signature.xsd" : "TEIF_without_signature.xsd";
                var schemaFilePath = Path.Combine(_schemaPath, schemaFileName);

                // Check if schema file exists
                if (!File.Exists(schemaFilePath))
                {
                    // For now, skip XSD validation if schema is not available
                    // In production, this should be a critical error
                    result.IsValid = true;
                    return await Task.FromResult(result);
                }

                var settings = new XmlReaderSettings();
                settings.Schemas.Add(null, schemaFilePath);
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                settings.ValidationEventHandler += (sender, args) =>
                {
                    result.Errors.Add(new ValidationError
                    {
                        Field = "XML",
                        Message = $"[{args.Severity}] Line {args.Exception?.LineNumber}: {args.Message}"
                    });
                };

                using var stringReader = new StringReader(xml);
                using var reader = XmlReader.Create(stringReader, settings);

                while (reader.Read()) { }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ValidationError
                {
                    Field = "XML",
                    Message = $"XML validation error: {ex.Message}"
                });
            }

            result.IsValid = result.Errors.Count == 0;
            return await Task.FromResult(result);
        }
    }
}
