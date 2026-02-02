namespace TunisianEInvoice.Domain.Entities
{
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
}
