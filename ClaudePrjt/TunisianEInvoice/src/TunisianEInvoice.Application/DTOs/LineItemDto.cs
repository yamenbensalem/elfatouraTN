namespace TunisianEInvoice.Application.DTOs
{
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
}
