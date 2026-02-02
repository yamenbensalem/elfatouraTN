namespace TunisianEInvoice.Domain.Entities
{
    public class InvoiceHeader
    {
        public PartnerIdentifier SenderIdentifier { get; set; }
        public PartnerIdentifier ReceiverIdentifier { get; set; }
    }
}
