namespace TunisianEInvoice.Domain.Entities
{
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
}
