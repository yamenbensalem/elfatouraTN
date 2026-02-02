using System.Collections.Generic;

namespace TunisianEInvoice.Domain.Entities
{
    public class InvoiceBody
    {
        public DocumentInfo DocumentInfo { get; set; }
        public DateInfo DateInfo { get; set; }
        public List<Partner> Partners { get; set; }
        public List<PaymentSection> PaymentSections { get; set; }
        public List<string> FreeTexts { get; set; }
        public List<string> SpecialConditions { get; set; }
        public List<LineItem> LineItems { get; set; }
        public InvoiceAmounts Amounts { get; set; }
        public List<TaxDetails> Taxes { get; set; }
    }
}
