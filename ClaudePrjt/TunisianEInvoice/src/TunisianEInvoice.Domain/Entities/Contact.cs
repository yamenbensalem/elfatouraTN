namespace TunisianEInvoice.Domain.Entities
{
    public class Contact
    {
        public string FunctionCode { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string CommunicationType { get; set; } // I-101 (Phone), I-102 (Fax), I-104 (Web)
        public string CommunicationAddress { get; set; }
    }
}
