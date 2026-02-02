namespace TunisianEInvoice.Domain.Entities
{
    public class TaxInfo
    {
        public string TaxTypeCode { get; set; }
        public string TaxTypeName { get; set; }
        public decimal TaxRate { get; set; }
    }
}
