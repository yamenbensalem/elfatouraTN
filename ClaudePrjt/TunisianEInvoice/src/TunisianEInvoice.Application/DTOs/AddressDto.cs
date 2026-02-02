namespace TunisianEInvoice.Application.DTOs
{
    public class AddressDto
    {
        public string Description { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; } // "TN"
        public string Language { get; set; } // "fr"
    }
}
