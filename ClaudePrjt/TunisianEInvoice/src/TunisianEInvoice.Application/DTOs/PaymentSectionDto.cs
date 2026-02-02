namespace TunisianEInvoice.Application.DTOs
{
    public class PaymentSectionDto
    {
        public string PaymentTermsTypeCode { get; set; } // "I-114" (Bank), "I-115" (Post)
        public string PaymentTermsDescription { get; set; }
        public BankAccountDto BankAccount { get; set; }
    }
}
