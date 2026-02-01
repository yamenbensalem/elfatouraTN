// TunisianEInvoice.Domain/Entities/Invoice.cs
using System;
using System.Collections.Generic;

namespace TunisianEInvoice.Domain.Entities
{
    public class Invoice
    {
        public string Version { get; set; } = "1.8.8";
        public string ControllingAgency { get; set; } = "TTN";
        
        public InvoiceHeader Header { get; set; }
        public InvoiceBody Body { get; set; }
        public RefTtnVal TtnValidation { get; set; }
    }

    public class InvoiceHeader
    {
        public PartnerIdentifier SenderIdentifier { get; set; }
        public PartnerIdentifier ReceiverIdentifier { get; set; }
    }

    public class InvoiceBody
    {
        public DocumentInfo DocumentInfo { get; set; }
        public DateInfo DateInfo { get; set; }
        public List<Partner> Partners { get; set; }
        public List<PaymentSection> PaymentSections { get; set; }
        public List<string> FreeTexts { get; set; }
        public List<string> SpecialConditions { get; set; }
        public List<LineItem> LineItems { get; set; }
        public InvoiceAmounts Amounts { get; set; }
        public List<TaxDetails> Taxes { get; set; }
    }

    public class DocumentInfo
    {
        public string DocumentIdentifier { get; set; }
        public string DocumentTypeCode { get; set; }
        public string DocumentTypeName { get; set; }
    }

    public class DateInfo
    {
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PeriodFrom { get; set; }
        public string PeriodTo { get; set; }
    }

    public class Partner
    {
        public string FunctionCode { get; set; } // I-62 (Sender), I-64 (Receiver)
        public PartnerIdentifier Identifier { get; set; }
        public string Name { get; set; }
        public string NameType { get; set; } // "Qualification"
        public Address Address { get; set; }
        public List<Reference> References { get; set; }
        public List<Contact> Contacts { get; set; }
    }

    public class Address
    {
        public string Description { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string Language { get; set; }
    }

    public class Reference
    {
        public string RefId { get; set; }
        public string Value { get; set; }
    }

    public class Contact
    {
        public string FunctionCode { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string CommunicationType { get; set; } // I-101 (Phone), I-102 (Fax), I-104 (Web)
        public string CommunicationAddress { get; set; }
    }

    public class LineItem
    {
        public string ItemIdentifier { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public string Language { get; set; }
        public decimal Quantity { get; set; }
        public string MeasurementUnit { get; set; }
        public TaxInfo Tax { get; set; }
        public LineAmounts Amounts { get; set; }
    }

    public class TaxInfo
    {
        public string TaxTypeCode { get; set; }
        public string TaxTypeName { get; set; }
        public decimal TaxRate { get; set; }
    }

    public class LineAmounts
    {
        public decimal UnitPriceExcludingTax { get; set; }
        public decimal TotalExcludingTax { get; set; }
    }

    public class PaymentSection
    {
        public string TermsTypeCode { get; set; }
        public string TermsDescription { get; set; }
        public BankAccount BankAccount { get; set; }
    }

    public class BankAccount
    {
        public string FunctionCode { get; set; }
        public string AccountNumber { get; set; }
        public string OwnerIdentifier { get; set; }
        public string BankCode { get; set; }
        public string BranchIdentifier { get; set; }
        public string InstitutionName { get; set; }
    }

    public class InvoiceAmounts
    {
        public decimal Capital { get; set; } // Montant du capital social
        public decimal TotalIncludingTax { get; set; }
        public decimal TotalExcludingTax { get; set; }
        public decimal TotalTaxableBase { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal StampDuty { get; set; }
        public string AmountInWords { get; set; }
    }

    public class TaxDetails
    {
        public string TaxTypeCode { get; set; }
        public string TaxTypeName { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxableBase { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class RefTtnVal
    {
        public string TtnReference { get; set; }
        public string QrCodeBase64 { get; set; }
        public DateTime ValidationDate { get; set; }
    }
}

// TunisianEInvoice.Domain/ValueObjects/PartnerIdentifier.cs
namespace TunisianEInvoice.Domain.Entities
{
    public class PartnerIdentifier
    {
        public string Type { get; set; } // I-01, I-02, I-03, I-04
        public string Value { get; set; }

        public bool IsValid()
        {
            return Type switch
            {
                "I-01" => IsValidMatriculeFiscale(Value),
                "I-02" => IsValidCIN(Value),
                "I-03" => IsValidCarteSejourValue),
                _ => !string.IsNullOrEmpty(Value)
            };
        }

        private bool IsValidMatriculeFiscale(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 13)
                return false;
            
            // Format: 7 chiffres + 1 lettre + 1 lettre + 1 lettre + 3 zéros
            return System.Text.RegularExpressions.Regex.IsMatch(
                value, 
                @"^[0-9]{7}[ABCDEFGHJKLMNPQRSTVWXYZ][ABDNP][CMNP][0]{3}$"
            );
        }

        private bool IsValidCIN(string value)
        {
            return !string.IsNullOrEmpty(value) && 
                   value.Length == 8 && 
                   System.Text.RegularExpressions.Regex.IsMatch(value, @"^[0-9]{8}$");
        }

        private bool IsValidCarteSejour(string value)
        {
            return !string.IsNullOrEmpty(value) && 
                   value.Length == 9 && 
                   System.Text.RegularExpressions.Regex.IsMatch(value, @"^[0-9]{9}$");
        }
    }
}
