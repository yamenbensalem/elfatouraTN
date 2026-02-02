using System;
using System.Collections.Generic;

namespace TunisianEInvoice.Domain.Entities
{
    /// <summary>
    /// Client entity - represents a company/client managed by the accountant
    /// </summary>
    public class Client
    {
        public Guid Id { get; set; }
        public string MatriculeFiscal { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? LegalForm { get; set; } // SA, SARL, etc.
        public string? RegistrationNumber { get; set; }
        public decimal? Capital { get; set; }
        
        // Address
        public string? AddressDescription { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string CountryCode { get; set; } = "TN";
        
        // Contact
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        
        // Bank info
        public string? BankAccountNumber { get; set; }
        public string? BankCode { get; set; }
        public string? BankName { get; set; }
        
        // TTN info
        public string? TtnAccountMode { get; set; } // SMTP
        public string? TtnAccountRank { get; set; } // P
        public string? TtnProfile { get; set; }
        public string? TtnClientCode { get; set; }
        
        // Certificate for signing
        public byte[]? SigningCertificate { get; set; }
        public string? CertificatePassword { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation
        public ICollection<InvoiceRecord> SentInvoices { get; set; } = new List<InvoiceRecord>();
        public ICollection<InvoiceRecord> ReceivedInvoices { get; set; } = new List<InvoiceRecord>();
    }
}
