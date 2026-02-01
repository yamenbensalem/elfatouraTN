// TunisianEInvoice.Application/DTOs/InvoiceRequestDto.cs
using System;
using System.Collections.Generic;

namespace TunisianEInvoice.Application.DTOs
{
    public class InvoiceRequestDto
    {
        public string DocumentIdentifier { get; set; } // "12016_2012"
        public string DocumentType { get; set; } // "I-11" (Facture)
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PeriodFrom { get; set; } // "010512"
        public string PeriodTo { get; set; } // "310512"
        
        public PartnerDto Sender { get; set; }
        public PartnerDto Receiver { get; set; }
        
        public List<LineItemDto> LineItems { get; set; }
        public List<PaymentSectionDto> PaymentSections { get; set; }
        
        public string FreeText { get; set; }
        public List<string> SpecialConditions { get; set; }
    }

    public class PartnerDto
    {
        public string IdentifierType { get; set; } // "I-01" (Matricule Fiscal)
        public string Identifier { get; set; } // "0736202XAM000"
        public string Name { get; set; }
        public AddressDto Address { get; set; }
        public List<ContactDto> Contacts { get; set; }
        
        // Pour le destinataire
        public string AccountMode { get; set; } // "SMTP"
        public string AccountRank { get; set; } // "P"
        public string Profile { get; set; } // "Salle Publique"
        public string ClientCode { get; set; } // "41115530"
        public string RegistrationNumber { get; set; } // "B154702000"
        public string LegalForm { get; set; } // "SA"
    }

    public class AddressDto
    {
        public string Description { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; } // "TN"
        public string Language { get; set; } // "fr"
    }

    public class ContactDto
    {
        public string Type { get; set; } // "I-101" (Phone), "I-102" (Fax), "I-104" (Web)
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class LineItemDto
    {
        public string ItemIdentifier { get; set; } // "1"
        public string ItemCode { get; set; } // "DDM"
        public string ItemDescription { get; set; }
        public decimal Quantity { get; set; }
        public string MeasurementUnit { get; set; } // "UNIT"
        public decimal UnitPriceExcludingTax { get; set; }
        public decimal TaxRate { get; set; } // 12
        public string TaxType { get; set; } // "I-1602" (TVA)
        public decimal TotalExcludingTax { get; set; }
        public string Language { get; set; } // "fr"
    }

    public class PaymentSectionDto
    {
        public string PaymentTermsTypeCode { get; set; } // "I-114" (Bank), "I-115" (Post)
        public string PaymentTermsDescription { get; set; }
        public BankAccountDto BankAccount { get; set; }
    }

    public class BankAccountDto
    {
        public string AccountNumber { get; set; }
        public string OwnerIdentifier { get; set; }
        public string BankCode { get; set; }
        public string BranchIdentifier { get; set; }
        public string InstitutionName { get; set; }
        public string FunctionCode { get; set; } // "I-141"
    }

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

    public class ValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
}
