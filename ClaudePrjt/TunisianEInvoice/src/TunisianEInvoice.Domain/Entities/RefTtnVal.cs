using System;

namespace TunisianEInvoice.Domain.Entities
{
    public class RefTtnVal
    {
        public string TtnReference { get; set; }
        public string QrCodeBase64 { get; set; }
        public DateTime ValidationDate { get; set; }
    }
}
