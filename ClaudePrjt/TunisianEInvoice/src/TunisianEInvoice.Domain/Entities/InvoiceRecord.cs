using System;
using System.Collections.Generic;

namespace TunisianEInvoice.Domain.Entities
{
    /// <summary>
    /// Invoice record stored in database - represents a generated/sent invoice
    /// </summary>
    public class InvoiceRecord
    {
        public Guid Id { get; set; }
        public int InvoiceNumber { get; set; } // Sequential number per year/sender
        public string DocumentIdentifier { get; set; } = string.Empty;
        public string DocumentTypeCode { get; set; } = "I-11"; // Facture
        public string DocumentTypeName { get; set; } = "Facture";
        
        // Dates
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PeriodFrom { get; set; }
        public string? PeriodTo { get; set; }
        
        // Sender (usually the client managed by accountant)
        public Guid SenderId { get; set; }
        public Client Sender { get; set; } = null!;
        
        // Receiver
        public Guid ReceiverId { get; set; }
        public Client Receiver { get; set; } = null!;
        
        // Amounts
        public decimal TotalExcludingTax { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal StampDuty { get; set; }
        public decimal TotalIncludingTax { get; set; }
        public string? AmountInWords { get; set; }
        
        // Status
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public string? StatusMessage { get; set; }
        
        // TTN Validation
        public string? TtnReference { get; set; }
        public DateTime? TtnValidationDate { get; set; }
        public string? QrCodeBase64 { get; set; }
        
        // Generated files
        public string? XmlWithoutSignature { get; set; }
        public string? XmlWithSignature { get; set; }
        public byte[]? PdfDocument { get; set; }
        
        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public string? CreatedBy { get; set; }
        
        // Navigation
        public ICollection<InvoiceLineRecord> LineItems { get; set; } = new List<InvoiceLineRecord>();
        public ICollection<InvoiceTaxRecord> Taxes { get; set; } = new List<InvoiceTaxRecord>();
    }
    
    public enum InvoiceStatus
    {
        Draft = 0,
        Generated = 1,
        Signed = 2,
        Sent = 3,
        Pending = 4,
        Validated = 5,
        Rejected = 6,
        Cancelled = 7
    }
}
