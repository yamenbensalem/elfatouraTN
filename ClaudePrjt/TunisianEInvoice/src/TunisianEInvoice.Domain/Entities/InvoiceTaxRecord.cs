using System;

namespace TunisianEInvoice.Domain.Entities
{
    /// <summary>
    /// Invoice tax summary stored in database
    /// </summary>
    public class InvoiceTaxRecord
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public InvoiceRecord Invoice { get; set; } = null!;
        
        public string TaxTypeCode { get; set; } = string.Empty;
        public string TaxTypeName { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public decimal TaxableBase { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
