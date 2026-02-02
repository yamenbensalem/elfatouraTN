namespace TunisianEInvoice.Domain.Entities
{
    public class Invoice
    {
        public string Version { get; set; } = "1.8.8";
        public string ControllingAgency { get; set; } = "TTN";
        
        public InvoiceHeader Header { get; set; }
        public InvoiceBody Body { get; set; }
        public RefTtnVal TtnValidation { get; set; }
    }
}
