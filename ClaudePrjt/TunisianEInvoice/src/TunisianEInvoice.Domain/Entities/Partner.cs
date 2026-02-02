using System.Collections.Generic;

namespace TunisianEInvoice.Domain.Entities
{
    public class Partner
    {
        public string FunctionCode { get; set; } // I-62 (Sender), I-64 (Receiver)
        public PartnerIdentifier Identifier { get; set; }
        public string Name { get; set; }
        public string NameType { get; set; } // "Qualification"
        public Address Address { get; set; }
        public List<Reference> References { get; set; }
        public List<Contact> Contacts { get; set; }
    }
}
