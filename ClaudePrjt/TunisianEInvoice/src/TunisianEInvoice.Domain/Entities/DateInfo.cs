using System;

namespace TunisianEInvoice.Domain.Entities
{
    public class DateInfo
    {
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PeriodFrom { get; set; }
        public string PeriodTo { get; set; }
    }
}
