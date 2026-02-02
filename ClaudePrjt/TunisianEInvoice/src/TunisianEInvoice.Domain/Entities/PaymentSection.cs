namespace TunisianEInvoice.Domain.Entities
{
    public class PaymentSection
    {
        public string TermsTypeCode { get; set; }
        public string TermsDescription { get; set; }
        public BankAccount BankAccount { get; set; }
    }
}
