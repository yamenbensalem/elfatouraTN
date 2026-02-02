namespace TunisianEInvoice.Application.DTOs
{
    public class ContactDto
    {
        public string Type { get; set; } // "I-101" (Phone), "I-102" (Fax), "I-104" (Web)
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
