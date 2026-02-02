using System;

namespace TunisianEInvoice.Domain.Entities
{
    /// <summary>
    /// Invoice line item stored in database
    /// </summary>
    public class InvoiceLineRecord
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public InvoiceRecord Invoice { get; set; } = null!;
        
        public int LineNumber { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
        public string Language { get; set; } = "fr";
        
        public decimal Quantity { get; set; }
        public string MeasurementUnit { get; set; } = "UNIT";
        
        public decimal UnitPriceExcludingTax { get; set; }
        public decimal TotalExcludingTax { get; set; }
        
        public string TaxTypeCode { get; set; } = "I-1602"; // TVA
        public string TaxTypeName { get; set; } = "TVA";
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
