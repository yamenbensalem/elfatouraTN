using System;
using System.Collections.Generic;

namespace TunisianEInvoice.Application.DTOs
{
    public class InvoiceRequestDto
    {
        public string DocumentIdentifier { get; set; } // "12016_2012"
        public string DocumentType { get; set; } // "I-11" (Facture)
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PeriodFrom { get; set; } // "010512"
        public string PeriodTo { get; set; } // "310512"
        
        public PartnerDto Sender { get; set; }
        public PartnerDto Receiver { get; set; }
        
        public List<LineItemDto> LineItems { get; set; }
        public List<PaymentSectionDto> PaymentSections { get; set; }
        
        public string FreeText { get; set; }
        public List<string> SpecialConditions { get; set; }
    }
}
