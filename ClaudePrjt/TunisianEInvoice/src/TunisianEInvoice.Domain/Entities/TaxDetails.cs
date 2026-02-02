namespace TunisianEInvoice.Domain.Entities
{
    public class TaxDetails
    {
        public string TaxTypeCode { get; set; }
        public string TaxTypeName { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxableBase { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
